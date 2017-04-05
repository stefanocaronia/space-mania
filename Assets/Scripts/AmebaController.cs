using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Shushao;

public class AmebaController : ScriptComponent, iPoolable {

	public EntityType EntityType = EntityType.Ameba;

	// oggetti
	PolygonCollider2D polygonCollider;

	const int numPieces = 2;

	Dictionary<MonsterSize, float> _cargoSizes = new Dictionary<MonsterSize, float> {
		{ MonsterSize.LITTLE, 80.0f },
		{ MonsterSize.NORMAL, 160.0f },
		{ MonsterSize.BIG, 220.0f },
		{ MonsterSize.HUGE, 1200.0f }
	};

	public float Hazard = 8.0f;

	private MonsterSize _size = MonsterSize.LITTLE;

	public MonsterSize Size {
		get {
			return _size;
		}
		set {			
			assemble(value);
			_size = value;		
		}
	}

	#region iPoolable implementation

	public void Initialize() {}
	public void Deactivate() {}
	public Pool Source { get; set; }

	#endregion

	void Awake() {	
		InitScriptComponent();
	}

	// Use this for initialization
	void Start() {
	}
	
	// Update is called once per frame
	void Update() {

		if (CARGO.IsFull() && Size < MonsterSize.HUGE) {
			Size++;
		}
	
	}

	public void OnExplode() {

		var pieces = new GameObject[numPieces];

		if (Size > MonsterSize.LITTLE) { // se non è già un pezzo piccolo, lo spezzo in 2 più piccoli

			Vector2 normal = Vector3.Cross(RIGIDBODY.velocity, Vector3.forward);
			normal.Normalize();

			for (int key = 0; key < pieces.Length; key++) {
				pieces[key] = create(WorldController.Instance.AmebaPool, transform.position + (Vector3)normal * (key == 0 ? -1.0f : 1.0f) * (Sprite.bounds.size.x / 4), transform.rotation, Size - 1);
			}
		} 
	}

	static public GameObject create(Pool sourcePool, Vector2 position, Quaternion rotation, MonsterSize size) {
		GameObject obj;
		obj = sourcePool.Get(position, rotation);
		obj.transform.parent = WorldController.Instance.AIFold.transform;
		obj.name = "Ameba " + obj.GetComponent<AmebaController>().Size.ToString();
		obj.GetComponent<AmebaController>().Size = size;
		WorldController.Instance.registerEntity(obj, EntityType.Ameba);
		return obj;
	}

	public void assemble(MonsterSize size) {
		
		Sprite[] textures;
		string fileName;

		fileName = "ameba-" + size.ToString().ToLower();
		textures = Resources.LoadAll<Sprite>("Sprites/" + fileName);

		if (textures.Length > 0 && Sprite != null)
			Sprite.sprite = textures[0];

		Animator.Play(fileName);

		updateCollider();

		RIGIDBODY.mass = (int)Size;
		DAMAGEABLE.hullCapacity = RIGIDBODY.mass * 10.0f;
		DAMAGEABLE.hull = DAMAGEABLE.hullCapacity;

		SHIP.Bounty = 10.0f;

		CARGO.Capacity = _cargoSizes[Size];
	}

	public void updateCollider() {
		if (GetComponent<PolygonCollider2D>() != null)
			Destroy(GetComponent<PolygonCollider2D>());
		polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();	
		polygonCollider.sharedMaterial = Resources.Load("Materials/Rock") as PhysicsMaterial2D;
	}

	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.GetComponent<Damageable>() != null && other.gameObject.GetComponent<AmebaController>() == null && other.gameObject.GetComponent<ItemController>() == null) {		
			other.gameObject.GetComponent<Damageable>().TakeDamage(Hazard * (int)Size, this.gameObject);		
		}		
	}

}