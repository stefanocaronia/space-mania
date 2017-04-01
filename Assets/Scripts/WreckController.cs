using UnityEngine;
using System.Collections.Generic;
using Shushao;

public class WreckController : ScriptComponent {

	// probabilità su 100 di comparsa dei singoli item
	Utility.Picker<ContentType> itemPicker = new Utility.Picker<ContentType>(new Dictionary<ContentType, int> {
		{ ContentType.Ice, 5 },
		{ ContentType.Iron, 2 },
		{ ContentType.Gold, 1 },
		{ ContentType.Uranium, 0 },
		{ ContentType.Parts, 40 },
		{ ContentType.Garbage, 20 },
		{ ContentType.Fuel, 20 },
		{ ContentType.Cell, 15 }
	});

	public Wallet Wallet;

	// Use this for initialization
	void Awake () {
		initScriptComponent();
		Sprite.sprite = Utility.getRandomSpriteInMultiple("Sprites/wrecks", 4);

		RegenerateCollider();
	}

	// Use this for initialization
	void Start () {
		Wallet = new Wallet(GetComponent<WreckController>());
		Wallet.Receive(Random.Range(0.0f, 30.0f));
		int numContainers = Random.Range(3, 4);
		for (int c = 0; c < numContainers; c++) {
			ContentType pickedType = itemPicker.Pick();
			CARGO.loadContainer(new CargoContainer(pickedType, Random.Range(6, 30)));
		}
	}

	static public GameObject create(Vector2 position, Quaternion rotation) {
		GameObject obj;
		obj = Instantiate(Resources.Load("Prefabs/Wreck"), position, rotation) as GameObject;
		obj.transform.parent = WorldController.Instance.itemsFold.transform;
		obj.name = "Wreck";
		WorldController.Instance.registerEntity(obj, EntityType.Wreck);
		return obj;
	}
}
