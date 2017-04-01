using UnityEngine;
using System.Collections;
using Shushao;

public class ItemController : ScriptComponent {

	string resourceName = "Item";

	public EntityType EntityType = EntityType.Item;
	public ContentType contentType;

	void Awake() {
		InitScriptComponent();
		if (hasCargo)
			contentType = CARGO.ContentType;
	}

	public void assemble() {
		if (hasCargo)
			resourceName = CARGO.ContentType.ToString();

		gameObject.name = resourceName;
	}

	static public GameObject create(Vector2 position, Quaternion rotation, ContentType type, float content, bool moveAtStart, bool rotateAtStart) {
		GameObject item;
		item = Instantiate(Resources.Load("Prefabs/" + type), position, rotation) as GameObject;
		item.GetComponent<Cargo>().ContentType = type;
		item.GetComponent<Cargo>().Quantity = content;
		item.GetComponent<Cargo>().Capacity = content;
		item.transform.parent = WorldController.Instance.itemsFold.transform;
		item.GetComponent<ItemController>().assemble();
		WorldController.Instance.registerEntity(item, EntityType.Item);
		return item;
	}
}
