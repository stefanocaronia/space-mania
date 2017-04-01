using UnityEngine;
using System.Collections.Generic;
using Shushao;

public class WorldController : SingletonBehaviour<WorldController> {

	#region CONFIGURAZIONE MONDO

	public float WIDTH = 120.0f; // width of the world in game units
	public float HEIGHT = 120.0f; // height of the world in game units
	public float asteroidDensity = 0.04f; // x unit
	public float wreckDensity = 0.005f; // x unit
	public float distanceBetweenEntities = 3.0f;
	public float celestialDensity = 0.001f; // x unit

	public Pool AsteroidsPool;
	public Pool ExplosionPool;
	public Pool ExplosionLightPool;
	public Pool SparksPool;
	public Pool MinerPool;
	public Pool TransportPool;
	public Pool PiratePool;
	public Pool AmebaPool;

	#endregion

	// oggetti
	public GameObject perimeter;
	public GameObject starfield;
	public GameObject asteroidsFold;
	public GameObject itemsFold;
	public GameObject celestialFold;
	public GameObject effectsFold;
	public GameObject AIFold;
	public GameObject StructuresFold;

	BoxCollider2D[] limits = new BoxCollider2D[4];

	float AREA;
	Rect boundaries;
	//private float cameraMargin = 4.0f;

	// sfondi tra cui scegliere a caso
	string[] starfieldImages = {
		"Sprites/starfield_01",
		"Sprites/starfield_02",
		"Sprites/starfield_03",
		"Sprites/starfield_04",
		"Sprites/starfield_05",
		"Sprites/starfield_06",
		"Sprites/starfield_07",
		"Sprites/starfield_08"
	};

	// immagini di corpi celesti e altri oggetti di background
	string[] celestialBodiesImages = {
		"Sprites/planet_01",
		"Sprites/planet_02",
		"Sprites/planet_03",
		"Sprites/planet_04"
	};

	//float[] itemDensity = new float[(int)System.Enum.GetNames(typeof(ContentType)).Length];

	// densità dei vari item generati
	Dictionary<ContentType, float> itemDensity = new Dictionary<ContentType, float> {
		{ ContentType.Fuel, 0.001f },
		{ ContentType.Cell, 0.001f },
		{ ContentType.Ice, 0.01f },
		{ ContentType.Iron, 0.005f },
		{ ContentType.Gold, 0.002f },
		{ ContentType.Uranium, 0.0002f },
		{ ContentType.Parts, 0.002f },
		{ ContentType.Garbage, 0.002f },
	};

	Dictionary<EntityType, float> entityDensity = new Dictionary<EntityType, float> {
		{ EntityType.Miner, 0.01f },
		{ EntityType.Transport, 0.002f },
		{ EntityType.Pirate, 0.003f },
		{ EntityType.Ameba, 0.002f }
	};

	// database contenente tutte le entity 
	[SerializeField]
	public EntityDB ENTITIES;

	public Rect Boundaries {
		get {
			return boundaries;
		}
	}

	public Pool GetPool(EntityType entity) {
		Pool pool = null;
		switch (entity) {
			case EntityType.Miner: 
				pool = MinerPool; 
				break;
			case EntityType.Transport: 
				pool = TransportPool; 
				break;
			case EntityType.Pirate: 
				pool = PiratePool; 
				break;
			case EntityType.Ameba: 
				pool = AmebaPool; 
				break;
		}
		return pool;
	}

	#region INIT

	// Use this for initialization
	void Awake() {

		// calcolo l'area dello starfield
		AREA = WIDTH * HEIGHT;

		ENTITIES = new EntityDB();

		// creo i limiti di gioco
		boundaries = new Rect();
		boundaries.width = WIDTH * 2;
		boundaries.height = HEIGHT * 2;
		boundaries.center = new Vector2(0.0f, 0.0f);

		limits = perimeter.GetComponents<BoxCollider2D>();

		limits[(int)Side.TOP].size = new Vector2(WIDTH * 2, 0.01f); // top
		limits[(int)Side.TOP].offset = new Vector2(0.0f, HEIGHT);
		limits[(int)Side.RIGHT].size = new Vector2(0.01f, HEIGHT * 2); // right
		limits[(int)Side.RIGHT].offset = new Vector2(WIDTH, 0.0f);
		limits[(int)Side.BOTTOM].size = limits[(int)Side.TOP].size; // bottom
		limits[(int)Side.BOTTOM].offset = new Vector2(0.0f, -HEIGHT);
		limits[(int)Side.LEFT].size = limits[(int)Side.RIGHT].size; // left
		limits[(int)Side.LEFT].offset = new Vector2(-WIDTH, 0.0f);

	}

	// Use this for initialization
	void Start() {
		AsteroidsPool.Populate();
		ExplosionPool.Populate();
		ExplosionLightPool.Populate();
		SparksPool.Populate();
		MinerPool.Populate();
		TransportPool.Populate();
		PiratePool.Populate();
	}

	public void generate() {

		if (!GameManager.Instance.Simulation) 
			generateStarfield();
		if (!GameManager.Instance.Simulation) 
			generateCelestials();

		spawnStations();

		spawnAsteroids();
		spawnWrecks();
		if (!GameManager.Instance.Simulation) 
			spawnNPC();

		spawnItems(ContentType.Fuel);
		spawnItems(ContentType.Cell);
		spawnItems(ContentType.Ice);
		spawnItems(ContentType.Iron);
		spawnItems(ContentType.Gold);
		spawnItems(ContentType.Uranium);
		spawnItems(ContentType.Parts);
		spawnItems(ContentType.Garbage);
	}

	#endregion

	#region UPDATE

	void Update() {

		/* Tentativo di bloccare la camera ai limits
		 * 
		 * if (Camera.main.transform.position.x - cameraMargin <= boundaries.xMin) {
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(boundaries.xMin + cameraMargin, Camera.main.transform.position.y, Camera.main.transform.position.z), Time.deltaTime * 1.2f);
		} else if  (Camera.main.transform.position.x + cameraMargin >= boundaries.xMax) {
					Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(boundaries.xMax - cameraMargin, Camera.main.transform.position.y, Camera.main.transform.position.z), Time.deltaTime * 1.2f);
		}
		if (Camera.main.transform.position.y - cameraMargin<= boundaries.yMin) {
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(Camera.main.transform.position.x, boundaries.yMin + cameraMargin, Camera.main.transform.position.z), Time.deltaTime * 1.2f);
		} else if  (Camera.main.transform.position.y + cameraMargin >= boundaries.yMax) {
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(Camera.main.transform.position.x, boundaries.yMax - cameraMargin, Camera.main.transform.position.z), Time.deltaTime * 1.2f);	
		}
		
		*/

	}

	#endregion

	#region GENERAZIONE SFONDO

	// genera lo sfondo
	public void generateStarfield() {
		GameObject first = generateTile(boundaries.x, boundaries.y, 0);
		Rect starfieldDim = first.GetComponent<SpriteRenderer>().sprite.rect;
		float pixelXunit = first.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;

		float x, y;
		int count = 0;
		float yStep = starfieldDim.height / pixelXunit;
		float xStep = starfieldDim.width / pixelXunit;
		for (y = boundaries.y; y <= boundaries.yMax + yStep; y += yStep) {
			for (x = boundaries.x; x <= boundaries.xMax + xStep; x += xStep) {
				if (!(x == boundaries.x && y == boundaries.y))
					generateTile(x, y, ++count);
			}		
		}	
	}

	// genera un tile di starfield alle coordinate indicate
	GameObject generateTile(float tx, float ty, int count) {
		GameObject tile = new GameObject();
		tile.AddComponent<SpriteRenderer>();
		SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
		sr.sprite = Resources.Load(starfieldImages[Random.Range(0, 7)], typeof(Sprite)) as Sprite;
		sr.flipX = (Random.value > 0.5f ? true : false);
		sr.flipY = (Random.value > 0.5f ? true : false);
		tile.transform.position = new Vector3(tx, ty, 0.0f);
		tile.transform.parent = starfield.transform;
		tile.name = "tile_" + count;
		return tile;
	}


	// genera cormi celesti
	public void generateCelestials() {
		int max = (int)Mathf.Ceil(AREA * celestialDensity);
		int min = (int)Mathf.Ceil(max * 0.8f);
		for (int i = 0; i < Random.Range(min, max); i++) {
			GameObject celestial = Instantiate(
				                       Resources.Load("Prefabs/Celestial Body"), 
				                       new Vector2(Random.Range(-WIDTH, WIDTH), Random.Range(-HEIGHT, HEIGHT)), 
				                       Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)) 
			                       ) as GameObject;
			SpriteRenderer sr = celestial.GetComponent<SpriteRenderer>();
			sr.sprite = Resources.Load(celestialBodiesImages[Random.Range(0, celestialBodiesImages.Length)], typeof(Sprite)) as Sprite;
			sr.flipX = (Random.value > 0.5f ? true : false);
			sr.flipY = (Random.value > 0.5f ? true : false);
			sr.color = new Color(Random.value, Random.value, Random.value, 1.0f);
			float newScale = Random.Range(1.0f, 3.0f);
			celestial.transform.localScale = new Vector3(newScale, newScale, 1.0f);
			celestial.transform.parent = celestialFold.transform;
			celestial.name = "Celestial Body";
			celestial.SetActive(true);
		}
	}

	#endregion

	#region GENERAZIONE CONTENUTI

	// genera gli asteroidi a secondo della densità
	public void spawnAsteroids() {		
		int max = (int)Mathf.Ceil(AREA * asteroidDensity);
		int min = (int)Mathf.Ceil(max * 0.8f);
		for (int i = 0; i < Random.Range(min, max); i++) {
			AsteroidController.create(
				AsteroidsPool,
				getRandomPosition(true), 
				Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)),
				(AsteroidSize)Random.Range(1, 3),
				true,
				true
			);
		}
	}

	// genera gli asteroidi a secondo della densità
	public void spawnWrecks() {		
		int max = (int)Mathf.Ceil(AREA * wreckDensity);
		int min = (int)Mathf.Ceil(max * 0.8f);
		for (int i = 0; i < Random.Range(min, max); i++) {
			WreckController.create(getRandomPosition(true), Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
		}
	}

	public void spawnNPC() {
		int max;
		int min;
		foreach (var pair in entityDensity) {
			max = (int)Mathf.Ceil(AREA * entityDensity[pair.Key]);
			min = (int)Mathf.Ceil(max * 0.8f);
			for (int i = 0; i < Random.Range(min, max); i++) {
				createNPC(pair.Key);
			}
		}
	}

	void createNPC(EntityType entity) {
		if (entity == EntityType.Ameba) {
			AmebaController.create(GetPool(entity), getRandomPosition(true), Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)), MonsterSize.LITTLE);
		} else {
			ShipController.create(GetPool(entity), getRandomPosition(true), Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
		}
	}

	public void spawnStations() {
		StationController.create(new Vector2(0.0f, 3.0f), Quaternion.identity);
		const int numStations = 3;
		for (int c = 1; c < numStations; c++) {
			StationController.create(getRandomPosition(true), Quaternion.identity);
		}
	}


	bool positionTooNearToEntities(Vector2 position) {
		List<Entity> tooNear = ENTITIES.FindAll(x => (x.position - position).magnitude < distanceBetweenEntities);
		return (tooNear.Count > 0);
	}

	Vector2 getRandomPosition(bool notToNear) {

		if (!notToNear)
			return  new Vector2(Random.Range(-WIDTH + 2, WIDTH - 2), Random.Range(-HEIGHT + 2, HEIGHT - 2));

		Vector2 pos = new Vector2();
		do {
			pos = new Vector2(Random.Range(-WIDTH + 2, WIDTH - 2), Random.Range(-HEIGHT + 2, HEIGHT - 2));
		} while (positionTooNearToEntities(pos));

		return pos;
	}

	// genera gli asteroidi a secondo della densità
	public void spawnItems(ContentType type) {		
		int max = (int)Mathf.Ceil(AREA * itemDensity[type]);
		int min = (int)Mathf.Ceil(max * 0.8f);
		for (int i = 0; i < Random.Range(min, max); i++) {
			ItemController.create(
				getRandomPosition(true), 
				Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)), 
				type,
				Random.Range(5, 20),
				true,
				true
			);
		}
	}

	#endregion

	public Entity registerEntity(GameObject go, EntityType type) {		
		Entity newEntity = new Entity();
		newEntity.ID = go.GetInstanceID();
		newEntity.name = go.name;
		newEntity.type = type;
		newEntity.position = go.transform.position;
		if (go.GetComponent<Cargo>() != null)
			newEntity.contentType = go.GetComponent<Cargo>().ContentType;
		newEntity.GameObject = go;
		ENTITIES.Add(newEntity);
		return newEntity;
	}

	public void unregisterEntity(GameObject go) {
		ENTITIES.removeByID(go.GetInstanceID());
	}

}
