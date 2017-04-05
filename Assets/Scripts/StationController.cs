using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Shushao;

public class StationController : MonoBehaviour {
	
	public bool Occupied;
	public bool Operating;
	public GameObject Ship;
	public Stack<GameObject> Queue { get; set; }

	ShipController _shipController;
	CircleCollider2D _tractorBeamActivationArea;
	Wallet Wallet;

	const float shipRotationSpeed = 1.0f;
	float RepairPrice = 0.5f;
	float FuelPrice = 0.2f;

	// valore delle merci
	Dictionary<ContentType, float> prices = new Dictionary<ContentType, float> {
		{ ContentType.Ice, 0.5f },
		{ ContentType.Iron, 1.0f },
		{ ContentType.Gold, 8.5f },
		{ ContentType.Uranium, 20.0f },
		{ ContentType.Passengers, 30.0f },
		{ ContentType.Garbage, 0.001f },
		{ ContentType.Parts, 5.0f }
	};

	// Use this for initialization
	void Awake() {
		_tractorBeamActivationArea = GetComponents<CircleCollider2D>()[1];
		Queue = new Stack<GameObject>();
	}

	void Start() {
		WorldController.Instance.registerEntity(this.gameObject, EntityType.Station);
	}

	// Update is called once per frame
	void Update() {
		if (Occupied && !Operating && Input.GetAxis("Action") > 0.0f) {
			StartCoroutine("undockShip");
		}
	}

	#region COROUTINES

	IEnumerator dockShip() {
		
		if (Operating)
			yield break;

		Operating = true;
		_tractorBeamActivationArea.enabled = false;
		Ship.GetComponent<PolygonCollider2D>().isTrigger = true;

		//const float distance = 1.2f;

		Vector3[] tappe = {
			//transform.position - transform.up * distance,
			transform.position
		};

		foreach (Vector3 tappa in tappe) {
			while (Ship.transform.position != tappa) {
				_shipController.LookAt(tappa);
				Ship.transform.position = Vector3.MoveTowards(Ship.transform.position, tappa, shipRotationSpeed * Time.deltaTime);
				yield return null;
			}
		}

		//guestShip.transform.rotation = transform.rotation;

		_shipController.Docked = true;

		Occupied = true;
		Operating = false;

		Ship.GetComponent<SpriteRenderer>().enabled = false;

		Wallet = (_shipController.isPlayerShip ? PlayerManager.Instance.Wallet : _shipController.Wallet);
		StartCoroutine(cStationOperations());
	}

	IEnumerator undockShip() {
		if (_shipController == null)
			yield break;

		_shipController.Docked = false;

		Operating = true;

		const float distance = 1.6f;

		Vector3 exitPoint = transform.position - transform.up * distance;

		Ship.GetComponent<SpriteRenderer>().enabled = true;

		while (Ship.transform.position != exitPoint) {
			_shipController.LookAt(exitPoint);
			Ship.transform.position = Vector3.MoveTowards(Ship.transform.position, exitPoint, shipRotationSpeed * Time.deltaTime);
			yield return null;
		}

		_tractorBeamActivationArea.enabled = true;
		_shipController.Autopilot = false;
		Ship.GetComponent<PolygonCollider2D>().isTrigger = false;

		if (!_shipController.isPlayerShip && Ship.GetComponent<AIController>() != null) {
			Ship.GetComponent<AIController>().Docking = false;
			Ship.GetComponent<AIController>().Action = AIAction.PATROL;
		}

		Wallet = null;
		Ship = null;
		_shipController = null;

		Occupied = false;
		Operating = false;

		// se ci sono navi in coda le docko
		if (Queue.Count > 0) {
			Dock(Queue.Pop());
		}
	}

	IEnumerator cStationOperations() {
		if (Ship == null || _shipController == null)
			yield break;

		if (_shipController !=null && Ship.GetComponent<Cargo>().containers.Count > 0)
			yield return StartCoroutine(cSellContainers());
		if (_shipController !=null && _shipController.DAMAGEABLE.hullIntegrity < 100.0f && Wallet.Credits > 0.0f)
			yield return StartCoroutine(cRepairDamage());
		if (_shipController !=null && _shipController.fuelPercent < 100.0f && Wallet.Credits > 0.0f)
			yield return StartCoroutine(cRefuel());
		if (_shipController !=null && _shipController.energyPercent < 100.0f)
			yield return StartCoroutine(cRecharge());

		if (_shipController !=null && !_shipController.isPlayerShip) {
			yield return new WaitForSeconds(2.0f);
			StartCoroutine("undockShip");
		}
	}

	IEnumerator cSellContainers() {
		if (!Occupied)
			yield break;

		Cargo cargo = Ship.GetComponent<Cargo>();

		while (Occupied && cargo.containers.Count > 0) {
			CargoContainer container = cargo.unloadContainer();

			if (_shipController.isPlayerShip) {
				PlayerManager.Instance.SellContainer(container, prices[container.contentType]);
			} else {
				_shipController.SellContainer(container, prices[container.contentType]);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	IEnumerator cRepairDamage() {
		if (!Occupied)
			yield break;

		float damage = _shipController.DAMAGEABLE.hullCapacity - _shipController.DAMAGEABLE.hull;
		print("riparo danni " + damage);
		float repairCost = Mathf.Clamp(damage * RepairPrice, 0.0f, Wallet.Credits);

		for (float c = 0; c < repairCost; c += 0.1f) {
			if (!Occupied)
				yield break;
			_shipController.DAMAGEABLE.Repair(0.1f * RepairPrice);
			Wallet.Pay(0.1f);
			yield return new WaitForSeconds(0.04f);
		}
	}

	IEnumerator cRefuel() {
		if (!Occupied)
			yield break;

		float fuelNeeded = _shipController.fuelCapacity - _shipController.Fuel;
		print("riempio serbatoio " + fuelNeeded);
		float fuelCost = Mathf.Clamp(fuelNeeded * FuelPrice, 0.0f, Wallet.Credits);
		print("costo fuel " + fuelCost);
		for (float c = 0; c < fuelCost; c += 0.1f) {
			if (!Occupied)
				yield break;
			_shipController.Refuel(0.1f * FuelPrice);
			Wallet.Pay(0.1f);
			yield return new WaitForSeconds(0.04f);
		}
	}

	IEnumerator cRecharge() {
		if (!Occupied)
			yield break;

		float consumedEnergy = _shipController.energyCapacity - _shipController.Energy;
		print("ricarico energia " + consumedEnergy);
		for (float c = 0; c < consumedEnergy; c += 0.2f) {
			if (!Occupied)
				yield break;
			_shipController.Recharge(c);
			yield return new WaitForSeconds(0.04f);
		}
	}

	#endregion

	public void OnDockRequest(GameObject ship) {
		if (Occupied || Operating) {
			if (!ship.GetComponent<ShipController>().isPlayerShip)
				Queue.Push(ship);
			return;
		}

		Dock(ship);
	}

	private void Dock(GameObject ship) {
		Ship = ship;
		if (ship.GetComponent<ShipController>() != null)
			_shipController = ship.GetComponent<ShipController>();
		else
			return;

		_shipController.Autopilot = true;

		if (!Operating)
			StartCoroutine("dockShip");
	}

	public bool Queued(GameObject ship) {
		if (Queue == null || Queue.Count == 0)
			return false;
		else
			return Queue.Contains(ship);
	}

	static public GameObject create(Vector2 position, Quaternion rotation) {
		GameObject obj;
		obj = Instantiate(Resources.Load("Prefabs/Station"), position, rotation) as GameObject;
		obj.transform.parent = WorldController.Instance.StructuresFold.transform;
		obj.name = "Station";
		WorldController.Instance.registerEntity(obj, EntityType.Station);
		return obj;
	}
}