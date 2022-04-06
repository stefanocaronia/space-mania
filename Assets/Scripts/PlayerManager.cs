using UnityEngine;
using Shushao;

public class PlayerManager : SingletonBehaviour<PlayerManager> {

	public GameObject Ship;
	
	public Wallet Wallet;

	[Range(-1,1)]
	public float Alignment = 0.0f;

	[HideInInspector]
	public ShipController ShipController;

	void Awake () {
		Ship = GameObject.FindWithTag("Player");
		ShipController = Ship.GetComponent<ShipController>();	
		Wallet = new Wallet(this);
	}

	void Start() {		
		GameManager.Instance.UI.updateCredits();
	}	

	public float SellContainer(CargoContainer container, float price) {
		float received = Wallet.Receive(price);
        GameManager.Instance.UI.updateCredits();
		GameManager.Instance.UI.updateCargoPanel();
		GameManager.Instance.UI.updateCargoFullness();
		return received;
	}
}
