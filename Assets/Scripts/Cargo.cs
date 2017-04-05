using UnityEngine;
using System.Collections.Generic;
using Shushao;

[RequireComponent(typeof(ShipController))]
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
			CountContainersContent();
			return Quantity + ContainersQuantity;
		}
	}

	public List<CargoContainer> containers = new List<CargoContainer>();

	private const float jettisonRate = 0.2f;
	private float jettisonTime;

	void Awake () {
		InitScriptComponent ();
		IsMultiContent = ContentType == ContentType.Mixed;
	}

	void Start() {
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
	}

	#region Single Content Methods

	// restituisce una quantità e la sottrae al contenuto
	public float unload(float amount) {
		
		if (IsMultiContent)
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
		return IsMultiContent ? 0.0f : unload(Quantity);		
	}

	// riceve una quantità e la aggiunge al contenuto (restitGameManager.Instance.UIsce quantità aggiunta)
	public float load(float amount) {
		if (IsMultiContent)
			return 0.0f;
		
		if (Quantity + amount > Capacity) {
			GenerateFloatingCargo (ContentType, Quantity + amount - Capacity);
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
	public void LoadContainer (CargoContainer container) {
		if ((TotalQuantity + container.quantity) > Capacity) {
			AddContainer(container);
			GenerateFloatingCargo (container.contentType, TotalQuantity + container.quantity - Capacity);
		} else {
			AddContainer (container);
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
		GenerateFloatingCargo (container);
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoFullness();
		if (isPlayerShip) 
			GameManager.Instance.UI.updateCargoPanel();
	} 

	// butta fuori un container con una parte del content
	public void Jettison(float amount) {
		float given = unload(amount);
		GenerateFloatingCargo (ContentType, given);
	}

	// genera un floating cargo di un tipo e quantità
	public void GenerateFloatingCargo(ContentType contentType, float amount) {
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
	public void GenerateFloatingCargo(CargoContainer container) {
		GenerateFloatingCargo (container.contentType, container.quantity);
	}

	// aggiunge un container al contenuto
	void AddContainer(CargoContainer container) {
		containers.Add (container);
	}

	// elimina tutti i container
	public void EmptyContainers() {
		containers.Clear();
		ContainersQuantity = 0.0f;
		if (isPlayerShip) {
			GameManager.Instance.UI.updateCargoPanel();
			GameManager.Instance.UI.updateCargoFullness();
		}
	}

	// trasferisce tutti i container (se possibile) a destCargo
	public bool TransferContainersToCargo(Cargo destCargo) {
		float destCargoContent = destCargo.TotalQuantity;
		int index = 0;
		bool loaded = false;
		List<CargoContainer> containers_clone = containers;
		foreach (CargoContainer container in containers_clone) {
			if (container.quantity + destCargoContent > destCargo.Capacity) {					
				continue;
			} else {
				containers.RemoveAt (index);
				destCargo.LoadContainer (container);
				loaded = true;
			}
			index++;
		}
		return loaded;
	}

	// trasferisce tutto il contenuto a destCargo
	public float TransferContentToCargo(Cargo destCargo) {
		if (destCargo.IsFull()) return 0.0f;
		float transfer = Mathf.Clamp(Quantity, 0.0f, destCargo.FreeSpace);
		if (transfer <= 0) return 0.0f;
		CargoContainer container = unloadContentAsContainer(transfer);
		destCargo.LoadContainer(container);
		return transfer;
	}


	public float FreeSpace {
		get {
			return Capacity - TotalQuantity;
		}
	}

    public bool IsMultiContent {
        get {
            return isMultiContent;
        }

        set {
            isMultiContent = value;
        }
    }

    // verifica se il container è vuoto
    public bool IsEmpty() {
		return (TotalQuantity <= 0);
	}

	// verifica se il container è pieno
	public bool IsFull() {
		if (IsMultiContent) {
			CountContainersContent();
		}
		return (TotalQuantity >= Capacity);
	}



	// conta il contenuto dei container
	public void CountContainersContent() {
		if (!IsMultiContent)
			return;
		ContainersQuantity = 0.0f;
		foreach (CargoContainer container in containers) {
			ContainersQuantity += container.quantity;
		}
	}

	// distrugge l'oggetto se è un item ed è vuoto
	public bool DestroyIfEmpty() {
		if (IsEmpty() && isItem) {
			Die ();
			return true;
		}
		return false;
	}

	public void GenerateContainers (Utility.Picker<ContentType> itemPicker, int minNumber, int maxNumber, int minContent, int maxContent) {
		int numContainers = Random.Range(minNumber, maxNumber);
		for (int c = 0; c < numContainers; c++) {
			ContentType pickedType = itemPicker.Pick();
			LoadContainer(new CargoContainer(pickedType, Random.Range(minContent, maxContent)));
		}
	}

	public float SellContainer(CargoContainer container, float price, Wallet wall) {
		float received = wall.Receive(price);
		return received;
	}


	public void OnExplode() {		
		if (CARGO.containers.Count > 0) {
			foreach (CargoContainer container in CARGO.containers) {
				CARGO.GenerateFloatingCargo(container);
			}
		}

		if (isShip && SHIP.Energy > 0) {
			GenerateFloatingCargo(new CargoContainer(ContentType.Cell, SHIP.Energy));
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
