﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Shushao;

public class ShipController : ScriptComponent, iPoolable {

	#region SHIP CONFIGURATION

	// SHIP CONFIGURATION
	public EntityType Type;
	public Sprite[] Versions;

	public float force = 3000.0f;
	public float firePower = 5.0f;
	public float fireRate = 0.2f;
	public float fireForce = 10.0f;
	public float fuelCapacity = 100.0f;
	public float energyCapacity = 100.0f;
	public float energyRechargeRate = 0.05f;
	public float rotationSpeed = 100.0f;
	public float fuelConsumption = 1.0f;
	public float shieldConsumption = 2.0f;
	public float fireConsumption = 1.0f;

	// oggetti
	public GameObject fire;
	public AudioClip shotAudio;
	public ParticleSystem reactors;

	#endregion

	[HideInInspector]
	public float cameraSpeed = 6.0f;
	[HideInInspector]
	public float energyPercent = 100.0f;
	[HideInInspector]
	public float fuelPercent = 100.0f;

	#region FIELDS

	[HideInInspector]
	public float Fuel;

	[HideInInspector]
	public float Energy;

	[HideInInspector]
	public bool Shield;

	[HideInInspector]
	public bool Exploding = false;

	// oggetti e componenti
	private AudioSource rocketsound;
	private AudioSource shotsound;
	private AudioSource shieldsound;
	private List<GameObject> fireSpawns = new List<GameObject>();
	private GameObject station;
	private Pool firePool;

	// variabili di movimento
	private float thrust = 0.0f;
	private float thrustH = 0.0f;
	private float thrustV = 0.0f;
	private float angle = 0.0f;
	private float turnForce = 0.0f;
	private float rotateAmount = 0.0f;
	private bool firePressed;
	private Vector2 facingDirection = Vector2.zero;
	private Vector2 facingPosition = Vector2.zero;

	// camera
	private Vector3 cameraOffset;

	// fire
	private float nextFire;

	// opzioni
	private const bool jettisonExceedingCargo = false;

	[HideInInspector]
	public bool FreeCamera = false;

	[HideInInspector]
	public bool Docked = false;

	[HideInInspector]
	public bool NearToStation = false;

	private bool autopilot = false;
	public bool Autopilot {
		get {
			return autopilot;
		}
		set {
			autopilot = value;
			if (GetComponent<InputController>() != null) GetComponent<InputController>().enabled = !autopilot;
			if (GetComponent<AIController>() != null) GetComponent<AIController>().enabled = !autopilot;
		}
	}

	// NPC CONFIGURATION

	public Wallet Wallet;
	public float Bounty = 0.0f;


	#endregion

	#region INIT

	#region iPoolable implementation

	public void Initialize() {
		
		if (!isPlayerShip) 
			NpcInitialization();
		
		SHIP.Wallet.Credits = 0.0f;
		DAMAGEABLE.FullRepair();
		GetComponent<Radar>().Enable();
	}

	public void Deactivate() {
		GetComponent<Radar>().Disable();
	}

	public Pool Source {get;set;}

	#endregion

	// Use this for initialization
	void Awake() {
		InitScriptComponent();

		// se esistono più sprite ne scelgo uno a caso
		if (Versions.Length > 0) {
			Sprite.sprite = Versions[Random.Range(0, Versions.Length -1)];
			RegenerateCollider();
		}

		Fuel = fuelCapacity;
		Energy = energyCapacity;

		foreach (Transform child in transform) {
			if (child.CompareTag("Turrets"))
				fireSpawns.Add(child.gameObject);
		}

		if (Sounds.Length > 0)
			rocketsound = Sounds[0];
		if (Sounds.Length > 1)
			shotsound = Sounds[1];
		if (Sounds.Length > 2)
			shieldsound = Sounds[2];

		if (transform.FindChild("Reactors") != null) reactors = transform.FindChild("Reactors").GetComponent<ParticleSystem>();

		firePool = new Pool(fire, 20, 10);
	}

	void Start() {
		if (isPlayerShip) {
			cameraOffset = transform.position - Camera.main.transform.position;
			GameManager.Instance.UI.updateEnergy();
			GameManager.Instance.UI.updateFuel();
		}

		WorldController.Instance.registerEntity(this.gameObject, Type);

		if (!isPlayerShip)
			NpcInitialization();
	}

	#endregion

	#region UPDATE

	void Update() {
		if (Exploding)
			return;

		if (Docked)
			return;

		if (Autopilot) {
			thrust = 0.0f;
			thrustH = 0.0f;
			thrustV = 0.0f;
			Shield = false;
			firePressed = false;
		}

		if (Shield) {
			if (Animator != null)
				Animator.SetBool("isShielded", true);
			thrust = 0.0f;
			thrustH = 0.0f;
			thrustV = 0.0f;
			firePressed = false;
			if (shieldsound != null && !shieldsound.isPlaying)
				shieldsound.Play();
			consumeEnergy(shieldConsumption * Time.deltaTime);
		} else {
			if (shieldsound != null)
				shieldsound.Stop();
			if (Animator != null)
				Animator.SetBool("isShielded", false);
		}

		if (!Shield && firePressed && Time.time > nextFire && Energy >= fireConsumption) {	
			nextFire = Time.time + fireRate;
			Attack();
		}

		if (!Shield && thrust > 0) {
			if (Fuel <= 0.0f && Energy >= fuelConsumption) {
				setEngines(EngineState.EMDRIVE);
				consumeEnergy(fuelConsumption * thrust * Time.deltaTime);
				thrust /= 8.0f;
			} else if (Fuel > 0.0f) {
					setEngines(EngineState.ON);
					burnFuel(fuelConsumption * thrust * Time.deltaTime);
				} else {
					setEngines(EngineState.OFF);
					thrust = 0.0f;
				}
		} else {
			setEngines(EngineState.OFF);
		}

		if (facingPosition != Vector2.zero) {
			angle = (180 / Mathf.PI) * Mathf.Atan2(facingPosition.y - transform.position.y, facingPosition.x - transform.position.x);
			angle = Utility.joy2objRot(angle);
		}

		if (!Autopilot && GetComponent<InputController>() != null && GetComponent<InputController>().MODE == InputController.InputMode.ROTDIR) {
			if (Mathf.Abs(rotateAmount) > 0.02f)
				transform.Rotate(0,0, transform.rotation.z + (rotateAmount * rotationSpeed * Time.deltaTime));
		} else {
			transform.rotation = Quaternion.Lerp(transform.rotation, Utility.getRotationFromAngle2D(angle), Time.deltaTime * rotationSpeed);
		}

		if (!FreeCamera && isPlayerShip) {
			Vector3 newPosition = transform.position - cameraOffset;
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, Time.deltaTime * cameraSpeed);

			// ROTATE CAMERA
			//Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, transform.rotation, Time.deltaTime * cameraSpeed);
		}

		Recharge(energyRechargeRate * Time.deltaTime);
	}

	void FixedUpdate() {
		if (Exploding)
			return;

		if (autopilot || Docked) {
			return;
		}

		if (!Shield && thrust > 0) {
			RIGIDBODY.AddForce(transform.up * thrust * force * Time.deltaTime);
		}

		//		if (!shieldActive && Mathf.Abs(thrustH) > 0.8f) {
		//			Vector2 forceH = (Vector3.right * thrustH * (force / 2)) - (transform.up * thrust * force);
		//			rb.AddForce (forceH * Time.deltaTime);
		//		}
		//
		//		if (!shieldActive && Mathf.Abs(thrustV) > 0.8f) {
		//			rb.AddForce ((Vector3.up - transform.up) * thrustV * (force/2) * Time.deltaTime);
		//		}
		//
		// posiziono la camera
	}

	#endregion

	#region ENGINES

	public float Recharge(float amount) {
		float exceed = (Energy + amount > energyCapacity ? Energy + amount - energyCapacity : 0.0f);
		Energy += amount;
		Energy = Mathf.Clamp(Energy, 0.0f, energyCapacity);
		energyPercent = (100 / energyCapacity) * Energy;
		if (isPlayerShip)
			GameManager.Instance.UI.updateEnergy();

		return exceed;
	}

	public void consumeEnergy(float amount) {
		Energy -= amount;
		Energy = Mathf.Clamp(Energy, 0.0f, energyCapacity);
		energyPercent = (100 / energyCapacity) * Energy;
		if (isPlayerShip)
			GameManager.Instance.UI.updateEnergy();
	}

	public void burnFuel(float amount) {
		Fuel -= amount;
		Fuel = Mathf.Clamp(Fuel, 0.0f, fuelCapacity);
		fuelPercent = (100 / fuelCapacity) * Fuel;
		if (isPlayerShip)
			GameManager.Instance.UI.updateFuel();
	}

	public float Refuel(float amount) {
		float exceed = (Fuel + amount > fuelCapacity ? Fuel + amount - fuelCapacity : 0.0f);
		Fuel += amount;
		Fuel = Mathf.Clamp(Fuel, 0.0f, fuelCapacity);
		fuelPercent = (100 / fuelCapacity) * Fuel;
		if (isPlayerShip)
			GameManager.Instance.UI.updateFuel();

		return exceed;
	}

	void setEngines(EngineState state) {
		switch (state) {
			case EngineState.OFF:
				//animator.SetBool("isFlying", false);
				if (rocketsound != null && rocketsound.isPlaying)
					rocketsound.Stop();
				if (reactors != null && reactors.isPlaying)
					reactors.Stop();
				break;
			case EngineState.EMDRIVE:
				//animator.SetBool ("isFlyingSlow", true);
				//animator.SetBool("isFlying", false);
				if (rocketsound != null && rocketsound.isPlaying)
					rocketsound.Stop();
				if (reactors != null && reactors.isPlaying)
					reactors.Stop();
				break;
			case EngineState.ON:
				//animator.SetBool("isFlying", true);
				if (rocketsound != null && !rocketsound.isPlaying)
					rocketsound.Play();
				if (reactors != null && !reactors.isPlaying)
					reactors.Play();
				break;
		}
	}

	#endregion

	#region COMANDI DI MOVIEMTO E AZIONE

	public void requestDock() {
		if (!NearToStation || station.GetComponent<StationController>().Queued(this.gameObject))
			return;
		station.GetComponent<StationController>().OnDockRequest(this.gameObject);
	}

	public void thrustForward(float inputThrust) {
		thrust = inputThrust;
	}

	public void thrustHorizontal(float inputThrust) {
		thrustH = inputThrust;
	}

	public void thrustVertical(float inputThrust) {
		thrustV = inputThrust;
	}

	public void setAngle(float inputAngle) {		
		angle = inputAngle;
		facingPosition = Vector2.zero;
	}

	public void SetShield(bool state) {
		Shield = state && Energy > 1.0f;
	}

	public void SetFirePressed(bool state) {
		firePressed = state;
	}

	public void Turn(float force) {
		turnForce = force;
	}

	public void Rotate(float speed) {
		rotateAmount = speed;
	}

	public void LookAt(Vector3 pos) {
		facingDirection = ((Vector2)pos - (Vector2)transform.position).normalized;
		facingPosition = pos;

		//StartCoroutine(faceDirection(facingDirection));
	}

	public void SetDirection(Vector2 dir) {
		facingDirection = dir;
	}

	public void Move(Vector3 pos) {
		LookAt(pos);
		StartCoroutine(cMoveToPosition(pos));
	}

	public void StoplookAt() {
		facingDirection = Vector3.zero;
		facingPosition = Vector3.zero;
	}

	public void Attack() {
		//print(gameObject.name + " SHOOTING");
		if (Exploding)
			return;

		if (autopilot || Docked) {
			return;
		}

		GameObject bolt;

		foreach (GameObject spawn in fireSpawns) {
			//((GameObject)Instantiate(Resources.Load("Prefabs/Light"), spawn.transform.position, spawn.transform.rotation)).transform.parent = transform;
			//bolt = (GameObject)Instantiate(fire, spawn.transform.position, spawn.transform.rotation);
			bolt = firePool.Get(spawn.transform.position, spawn.transform.rotation);
			bolt.GetComponent<Fire>().shooter = this.gameObject;
			bolt.GetComponent<Fire>().power = firePower;
			bolt.GetComponent<Fire>().force = fireForce + RIGIDBODY.velocity.magnitude * 10.0f;
			bolt.GetComponent<Fire>().Shot();
			if (shotsound != null)
				shotsound.PlayOneShot(shotAudio);
			consumeEnergy(fireConsumption);
			if (isPlayerShip)
				GameManager.Instance.UI.updateEnergy();
		}
	}

	#endregion

	#region COROUTINES

	private IEnumerator cMoveToPosition(Vector3 pos) {

		while (transform.position != pos) {
			LookAt(pos);
			transform.position = Vector3.MoveTowards(transform.position, pos, 1.0f * Time.deltaTime);
			yield return null;
		}

		//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
		yield return null;
	}


	private IEnumerator faceDirection(Vector3 direction) {
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
		yield return null;
	}

	private IEnumerator generateFloatingCargo(ContentType C, float Q, float delay) {
		yield return new WaitForSeconds(delay);
		CARGO.generateFloatingCargo(C, Q);
	}

	private IEnumerator attract(GameObject other) {
		// imposto il box collider dell'altro oggetto a trigger
		if (other.GetComponent<BoxCollider2D>() != null) {
			if (!other.GetComponent<BoxCollider2D>().isTrigger)
				other.GetComponent<BoxCollider2D>().isTrigger = true;
		} else if (other.GetComponent<PolygonCollider2D>() != null) {
				if (!other.GetComponent<PolygonCollider2D>().isTrigger)
					other.GetComponent<PolygonCollider2D>().isTrigger = true;
			}

		// finchè la distanza tra i due oggetti è > 0, sposto l'altro oggetto verso la nave e lo rimpicciolisco
		while (Vector2.Distance(other.transform.position, transform.position) > 0.05f) {
			other.transform.position = Vector2.Lerp(other.transform.position, transform.position, 6.0f * Time.deltaTime);
			other.transform.localScale = Vector3.Lerp(other.transform.localScale, new Vector3(0.0f, 0.0f, 0.0f), 6.0f * Time.deltaTime);
			yield return null;
		}

		// alla fine distruggo l'oggetto
		//other.gameObject.GetComponent<Cargo>().destroyIfEmpty();
		other.gameObject.GetComponent<Cargo>().Die();
		yield return null;
	}

	#endregion

	#region GESTIONE COLLISIONI

	// collide contro qualcosa
	void OnCollisionEnter2D(Collision2D other) {
		if (Exploding)
			return;

		if (autopilot || Docked) {
			return;
		}

		switch (other.gameObject.tag) {
			case "Asteroid":
				if (DAMAGEABLE.DamageFromCollisions) {
					DAMAGEABLE.TakeDamage(80.0f, other.gameObject);
				}
				if (other.gameObject.GetComponent<Damageable>() != null)
					other.gameObject.GetComponent<Damageable>().TakeDamage(5.0f, this.gameObject);
				
				break;
		}

		checkItemCollision(other.gameObject);
	}

	// il trigger collide contro qualcosa
	void OnTriggerEnter2D(Collider2D other) {
		if (Exploding)
			return;

		if (autopilot || Docked) {
			return;
		}

		checkItemCollision(other.gameObject);
	}

	// il trigger contiene qualcosa
	void OnTriggerStay2D(Collider2D other) {
		if (Exploding)
			return;

		if (other.tag == "Station") {
			NearToStation = true;
			station = other.gameObject;
		}

		if (autopilot || Docked) {
			return;
		}

		checkItemCollision(other.gameObject);
	}

	void OnTriggerExit2D(Collider2D other) {
		if (Exploding)
			return;

		if (other.tag == "Station") {
			NearToStation = false;
		}

	}

	// controllo le collisioni con item (fuel, cell, minerali etc)
	void checkItemCollision(GameObject item) {
		if (item.GetComponent<ItemController>() == null)
			return;

		if (autopilot || Docked) {
			return;
		}

		float taken;
		Cargo itemCargo = item.GetComponent<Cargo>();

		// controllo il tag dell'oggetto con cui è avvenuta la collisione
		switch (item.tag) {
			case "Fuel": // si tratta di un bidone di fuel

				if (GetComponent<AmebaController>() != null) {
					taken = itemCargo.transferContentToCargo(CARGO);
					if (taken > 0) itemCargo.Die();
					break;
				}
				
				if (Fuel >= fuelCapacity || !hasCargo) // || Time.time < nextItemCatch
                    break;
				
				if (itemCargo.ContentType != ContentType.Fuel)
					break;
				taken = itemCargo.unload(itemCargo.Quantity);

				if (taken > 0) {
					itemCargo.Die();
					if (GetComponent<Radar>() != null) GetComponent<Radar>().Reset();
				}
				/*if (itemCargo.isEmpty())
                {
                    StartCoroutine(attract(item));
                }*/

				float surplusFuel = Refuel(taken);
				if (surplusFuel > 0.0f && jettisonExceedingCargo) {					
					StartCoroutine(generateFloatingCargo(ContentType.Fuel, surplusFuel, 0.6f)); // butto fuori il contenuto eccedente
				}

				//nextItemCatch = Time.time + itemCatchRate;

				break;

			case "Cell": // si tratta di una energy cell

				if (GetComponent<AmebaController>() != null) {
					taken = itemCargo.transferContentToCargo(CARGO);
					if (taken > 0) itemCargo.Die();
					break;
				}

				if (Energy >= energyCapacity || !hasCargo) // || Time.time < nextItemCatch
                    break;

				if (itemCargo.ContentType != ContentType.Cell)
					break;
				taken = itemCargo.unload(itemCargo.Quantity);
				if (taken > 0) {
					itemCargo.Die();
					if (GetComponent<Radar>() != null) GetComponent<Radar>().Reset();
				}
                /*if (itemCargo.isEmpty())
                {
                    StartCoroutine(attract(item));
                }*/

				float surplusEnergy = Recharge(taken);
				if (surplusEnergy > 0.0f && jettisonExceedingCargo) {					
					StartCoroutine(generateFloatingCargo(ContentType.Cell, surplusEnergy, 0.6f)); // butto fuori il contenuto eccedente
				}

				//nextItemCatch = Time.time + itemCatchRate;

				break;

			case "MultiContent": // si tratta di un item multiContent
				if (!hasCargo || (hasCargo && CARGO.isFull())) // || Time.time < nextItemCatch
                    break;

				bool loaded = itemCargo.transferContainersToCargo(CARGO);

				if (loaded) {
					itemCargo.Die();
					if (GetComponent<Radar>() != null) GetComponent<Radar>().Reset();
				}

				break;

			case "Limits":
                //freeCamera = true;
                //rb.AddForce(-transform.up * 4.0f,ForceMode2D.Impulse);
				break;

			default: // tutte le altre collisioni con item
				
				if (!hasCargo || (hasCargo && CARGO.isFull())) // || Time.time < nextItemCatch
                    break;

				taken = itemCargo.transferContentToCargo(CARGO);
				if (taken > 0) {
					itemCargo.Die();
					if (GetComponent<Radar>() != null) GetComponent<Radar>().Reset();
				}

//                if (itemCargo.isEmpty()) {
//                    StartCoroutine(attract(item));
//                }

				if (isPlayerShip) {
					GameManager.Instance.UI.updateCargoPanel();
					GameManager.Instance.UI.updateCargoFullness();
				}

				//nextItemCatch = Time.time + itemCatchRate;

				break;
		}
	}

	#endregion

	static public GameObject create(EntityType entityType, Vector2 position, Quaternion rotation) {
		GameObject obj;
		obj = Instantiate(Resources.Load("Ships/" + entityType), position, rotation) as GameObject;
		obj.transform.parent = WorldController.Instance.AIFold.transform;
		obj.name = entityType.ToString();
		WorldController.Instance.registerEntity(obj, entityType);
		return obj;
	}

	static public GameObject create(Pool sourcePool,  Vector2 position, Quaternion rotation) {
		GameObject obj;
		obj = sourcePool.Get(position, rotation);
		obj.transform.parent = WorldController.Instance.AIFold.transform;
		WorldController.Instance.registerEntity(obj, obj.GetComponent<ShipController>().Type);
		return obj;
	}

	public float sellContainer(CargoContainer container, float price) {
		float received = Wallet.Receive(price);
		return received;
	}


	void NpcInitialization() {

		Wallet = new Wallet(this);

		switch (Type) {

			case EntityType.Miner:

				firePower = Random.Range(1.0f, 4.0f);
				force = Random.Range(800, 1200);
				CARGO.Capacity = Random.Range(20, 40);

				break;
			
			case EntityType.Transport:

				CARGO.Capacity = 200.0f;
				force = Random.Range(200, 500);

				Utility.Picker<ContentType> itemPicker = new Utility.Picker<ContentType>(new Dictionary<ContentType, int> {
					{ ContentType.Ice, 5 },
					{ ContentType.Iron, 2 },
					{ ContentType.Gold, 2 },
					{ ContentType.Uranium, 1 },
					{ ContentType.Parts, 10 },
					{ ContentType.Garbage, 20 },
					{ ContentType.Fuel, 20 },
					{ ContentType.Cell, 15 },
					{ ContentType.Passengers, 40 }
				});

				CARGO.GenerateContainers(itemPicker, 3, 4, 6, 30);
				Wallet.Receive(Random.Range(0.0f, 30.0f));

				break;
		}

		AI.Action = AIConfiguration.AITemplates[Type].StartAction;
	}
}