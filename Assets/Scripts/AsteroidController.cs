using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Shushao;

public class AsteroidController : ScriptComponent, iPoolable {

	public AsteroidSize size;

	public EntityType EntityType = EntityType.Asteroid;

	// oggetti
	PolygonCollider2D polygonCollider;

	// probabilità su 100 di comparsa dei singoli item
//	Dictionary<ContentType, int> itemProbabilityTable = new Dictionary<ContentType,int> {
//		{ ContentType.NONE, 20 },
//		{ ContentType.Ice, 50 },
//		{ ContentType.Iron, 20 },
//		{ ContentType.Gold, 8 },
//		{ ContentType.Uranium, 2 }
//	};
//
	Utility.Picker<ContentType> itemPicker = new Utility.Picker<ContentType>(new Dictionary<ContentType,int> {
		{ ContentType.Ice, 30 },
		{ ContentType.Iron, 20 },
		{ ContentType.Gold, 8 },
		{ ContentType.Uranium, 2 }
	});

	const int numPieces = 2;

	#region iPoolable implementation
	public void Initialize() {

	}

	public void Deactivate() {

	}

	public Pool Source { get; set; }

	#endregion

	#region INIT

	void Awake() {	
		InitScriptComponent();

		if (size > AsteroidSize.NONE)
			assemble();
	}

	void Start() {
		
	}

	#endregion

	public void OnExplode() {

		var pieces = new GameObject[numPieces];

		if (size < AsteroidSize.LITTLE) { // se non è già un pezzo piccolo, lo spezzo in 2 asteroidi più piccolo
			
			Vector2 normal = Vector3.Cross(RIGIDBODY.velocity, Vector3.forward);
			normal.Normalize();

			for (int key = 0; key < pieces.Length; key++) {
				pieces[key] = create(WorldController.Instance.AsteroidsPool,  transform.position + (Vector3)normal * (key == 0 ? -1.0f : 1.0f) * (Sprite.bounds.size.x / 4), transform.rotation, size + 1, false, true);
				pieces[key].GetComponent<Rigidbody2D>().AddForce(normal * (key == 0 ? -1.0f : 1.0f) * 2.0f, ForceMode2D.Impulse);
			}

		} else {
		
			int itemNumber = Random.Range(1, 3);

			for (int n = 0; n < itemNumber; n++) {
				ContentType pickedType = itemPicker.Pick();
				if (pickedType == ContentType.NONE)
					continue;
				ItemController.Create(
					transform.position,
					Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)),
					pickedType,
					Random.Range(5, 20),
					false,
					true
				);				
			}
		
		}
	}


	static public GameObject create(Vector2 position, Quaternion rotation, AsteroidSize size, bool moveAtStart, bool rotateAtStart) {
		GameObject asteroid;
		asteroid = Instantiate(Resources.Load("Prefabs/Asteroid"), position, rotation) as GameObject;
		asteroid.GetComponent<AsteroidController>().size = size;
		asteroid.transform.parent = WorldController.Instance.asteroidsFold.transform;
		asteroid.name = "Asteroid " + asteroid.GetComponent<AsteroidController>().size.ToString();
		asteroid.GetComponent<AsteroidController>().assemble();
		WorldController.Instance.registerEntity(asteroid, EntityType.Asteroid);
		return asteroid;
	}

	static public GameObject create(Pool sourcePool, Vector2 position, Quaternion rotation, AsteroidSize size, bool moveAtStart, bool rotateAtStart) {
		GameObject asteroid;
		asteroid = sourcePool.Get(position, rotation);
		asteroid.GetComponent<AsteroidController>().size = size;
		asteroid.transform.parent = WorldController.Instance.asteroidsFold.transform;
		asteroid.name = "Asteroid " + asteroid.GetComponent<AsteroidController>().size.ToString();
		asteroid.GetComponent<AsteroidController>().assemble();
		WorldController.Instance.registerEntity(asteroid, EntityType.Asteroid);
		return asteroid;
	}

	public void assemble() {
		switch (size) {
			case AsteroidSize.HUGE:
				if (Sprite != null)
					Sprite.sprite = Utility.getRandomSpriteInMultiple("Sprites/asteroids-huge", 4);
				break;
			case AsteroidSize.BIG:
				if (Sprite != null)
					Sprite.sprite = Utility.getRandomSpriteInMultiple("Sprites/asteroids-big", 4);
				break;
			case AsteroidSize.LITTLE:
				if (Sprite != null)
					Sprite.sprite = Utility.getRandomSpriteInMultiple("Sprites/asteroids-little", 4);
				break;				
		}
		if (GetComponent<PolygonCollider2D>() != null)
			Destroy(GetComponent<PolygonCollider2D>());
		polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();	
		polygonCollider.sharedMaterial = Resources.Load("Materials/Rock") as PhysicsMaterial2D;

		//damageable.hardiness = rb.mass / 6.0f;
		DAMAGEABLE.hullCapacity = RIGIDBODY.mass * 10.0f;
		DAMAGEABLE.hull = DAMAGEABLE.hullCapacity;

	}

	// collide contro qualcosa
	void OnCollisionEnter2D(Collision2D other) {

		switch (other.gameObject.tag) {
			case "Asteroid":
				DAMAGEABLE.TakeDamage(2.0f, other.gameObject);
				break;
		}
	}

}
