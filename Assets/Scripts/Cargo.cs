using UnityEngine;
using System.Collections.Generic;
using Shushao;

public class Cargo : ScriptComponent {

	public ContentType ContentType;

	private bool isMultiContent = false;

	// Stato del container
	public float Capacity = 20.0f;
	public float Quantity = 0.0f;
	public float ContainersQuantity = 0.0f;

	// contenuto dei container + il content
	public float TotalQuantity {
		get {
			countContainersContent();
			return Quantity + ContainersQuantity;
		}
	}

	public List<CargoContainer> containers = new List<CargoContainer>();

	private const float jettisonRate = 0.2f;
	private float jettisonTime;

	void Awake () {
		InitScriptComponent ();
		isMultiContent = ContentType == ContentType.Mixed;
	}

	void Start() {
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
	}

	#region Single Content Methods

	// restituisce una quantità e la sottrae al contenuto
	public float unload(float amount) {
		
		if (isMultiContent)
			return 0.0f;
		
		Quantity -= amount;
		if (Quantity < 0)
			amount += Quantity;

		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();

		return amount;
	}

	// restitGameManager.Instance.UIsce tutto il contenuto e lo azzera
	public float unload() {
		return isMultiContent ? 0.0f : unload(Quantity);		
	}

	// riceve una quantità e la aggiunge al contenuto (restitGameManager.Instance.UIsce quantità aggiunta)
	public float load(float amount) {
		if (isMultiContent)
			return 0.0f;
		
		if (Quantity + amount > Capacity) {
			generateFloatingCargo (ContentType, Quantity + amount - Capacity);
			amount = Capacity - Quantity;
			Quantity = Capacity;
		} else {
			Quantity += amount;
		}

		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
		
		return amount;
	}

	// TEST
	void OnMouseDown(){
		//jettison (2);
	}

	// restituisce un container con la quantità specificata
	public CargoContainer unloadContentAsContainer(float amount) {
		float given = unload(amount);
		return new CargoContainer (ContentType, given);
	}

	// restituisce un container con tutto il contenuto
	public CargoContainer unloadContentAsContainer() {
		float given = unload();
		return new CargoContainer(ContentType, given);
	}

	#endregion

	// carica un container nel multicontent (se si va oltre la capacity, butta fuori l'eccedenza come container)
	public void loadContainer (CargoContainer container) {
		if ((TotalQuantity + container.quantity) > Capacity) {
			addContainer(container);
			generateFloatingCargo (container.contentType, TotalQuantity + container.quantity - Capacity);
		} else {
			addContainer (container);
		}
	}

	// scarica e restitGameManager.Instance.UIsce l'ultimo container caricato
	public CargoContainer unloadContainer() {
		CargoContainer container = containers[containers.Count - 1];
		containers.RemoveAt (containers.Count - 1);

		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
		
		return container;
	}

	// scarica l'ultimo container e lo butta fuori
	public void jettisonContainer() {
		if (containers.Count == 0 ) return;
		if (jettisonTime>Time.time) return;
		jettisonTime = Time.time + jettisonRate;
		CargoContainer container = unloadContainer ();
		generateFloatingCargo (container);
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoPanel();
	} 

	// butta fuori un container con una parte del content
	public void jettison(float amount) {
		float given = unload(amount);
		generateFloatingCargo (ContentType, given);
	}

	// genera un floating cargo di un tipo e quantità
	public void generateFloatingCargo(ContentType contentType, float amount) {
		//shipController.nextItemCatch = Time.time + shipController.itemCatchRate;
		Quaternion randomRotation = Quaternion.Euler (0.0f, 0.0f, Random.Range (0.0f, 360.0f));
		Vector3 newPosition = -transform.up * 0.6f;
		GameObject jett = (GameObject) Instantiate (Resources.Load("Prefabs/"+contentType), transform.position + newPosition, randomRotation);
		jett.transform.parent = WorldController.Instance.itemsFold.transform;
		Cargo jettCargo = jett.GetComponent<Cargo> ();
		jettCargo.Capacity = amount;
		jettCargo.Quantity = amount;
		jettCargo.ContentType = contentType;
		jett.GetComponent<Mover>().rotateAtStart = true;
		jett.GetComponent<Mover>().moveAtStart = false;
		jett.GetComponent<Rigidbody2D>().AddForce(-transform.up * 3.0f);
		jett.tag = contentType.ToString();
	}

	// genera un floating cargo da un container
	public void generateFloatingCargo(CargoContainer container) {
		generateFloatingCargo (container.contentType, container.quantity);
	}

	// aggiunge un container al contenuto
	void addContainer(CargoContainer container) {
		containers.Add (container);
	}

	// elimina tutti i container
	public void emptyContainers() {
		containers.Clear();
		ContainersQuantity = 0.0f;
		if (isPlayerShip) {
			GameManager.Instance.UI.updateCargoPanel();
			GameManager.Instance.UI.updateCargoFullness();
		}
	}

	// trasferisce tutti i container (se possibile) a destCargo
	public bool transferContainersToCargo(Cargo destCargo) {
		float destCargoContent = destCargo.TotalQuantity;
		int index = 0;
		bool loaded = false;
		List<CargoContainer> containers_clone = containers;
		foreach (CargoContainer container in containers_clone) {
			if (container.quantity + destCargoContent > destCargo.Capacity) {					
				continue;
			} else {
				containers.RemoveAt (index);
				destCargo.loadContainer (container);
				loaded = true;
			}
			index++;
		}
		return loaded;
	}

	// trasferisce tutto il contenuto a destCargo
	public float transferContentToCargo(Cargo destCargo) {
		if (destCargo.isFull()) return 0.0f;
		float transfer = Mathf.Clamp(Quantity, 0.0f, destCargo.FreeSpace);
		if (transfer <= 0) return 0.0f;
		CargoContainer container = unloadContentAsContainer(transfer);
		destCargo.loadContainer(container);
		return transfer;
	}


	public float FreeSpace {
		get {
			return Capacity - TotalQuantity;
		}
	}

	// verifica se il container è vuoto
	public bool isEmpty() {
		return (TotalQuantity <= 0);
	}

	// verifica se il container è pieno
	public bool isFull() {
		if (isMultiContent) {
			countContainersContent();
		}
		return (TotalQuantity >= Capacity);
	}



	// conta il contenuto dei container
	public void countContainersContent() {
		if (!isMultiContent)
			return;
		ContainersQuantity = 0.0f;
		foreach (CargoContainer container in containers) {
			ContainersQuantity += container.quantity;
		}
	}

	// distrugge l'oggetto se è un item ed è vuoto
	public bool destroyIfEmpty() {
		if (isEmpty() && isItem) {
			Die ();
			return true;
		}
		return false;
	}

	public void GenerateContainers (Utility.Picker<ContentType> itemPicker, int minNumber, int maxNumber, int minContent, int maxContent) {
		int numContainers = Random.Range(minNumber, maxNumber);
		for (int c = 0; c < numContainers; c++) {
			ContentType pickedType = itemPicker.Pick();
			loadContainer(new CargoContainer(pickedType, Random.Range(minContent, maxContent)));
		}
	}

	public float sellContainer(CargoContainer container, float price, Wallet wall) {
		float received = wall.Receive(price);
		return received;
	}


	public void OnExplode() {		
		if (CARGO.containers.Count > 0) {
			foreach (CargoContainer container in CARGO.containers) {
				CARGO.generateFloatingCargo(container);
			}
		}

		if (isShip && SHIP.Energy > 0) {
			generateFloatingCargo(new CargoContainer(ContentType.Cell, SHIP.Energy));
		}	
	}

	// ottiene lo sprite corrispondente al content type
	//Sprite getItemSprite(Globals.ContentType type) {

		/* Snippet per caricare una texture da una sprite map
		 * 
		 * Sprite[] textures = Resources.LoadAll<Sprite>("Sprites/Items");
		int index = 0;
		switch (type) {
			case Globals.ContentType.Fuel:
				index = 0;
				break;
		}			
		return textures[index];
		*/
	//}

}
