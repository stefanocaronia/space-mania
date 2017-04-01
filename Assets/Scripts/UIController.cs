using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIController : MonoBehaviour {

	GameObject playerShip;
	GameObject cargoPanel;
	Text fuelText;
	Text energyText;
	Text integrityText;
	Text cargoFullness;
	Text creditsText;
	Text message;
	Cargo playerShipCargo;
	ShipController playerShipController;

	// Use this for initialization
	void Awake() {

		playerShip = PlayerManager.Instance.Ship;
		playerShipController = playerShip.GetComponent<ShipController>();
		playerShipCargo = playerShip.GetComponent<Cargo>();

		fuelText = transform.FindChild("Fuel").GetComponent<Text>();
		energyText = transform.FindChild("Energy").GetComponent<Text>();
		integrityText = transform.FindChild("Integrity").GetComponent<Text>();
		cargoPanel = transform.FindChild("Cargo").gameObject;
		cargoFullness = transform.FindChild("Fullness").gameObject.GetComponent<Text>();
		creditsText = transform.FindChild("Credits").gameObject.GetComponent<Text>();
		message = transform.FindChild("Message").gameObject.GetComponent<Text>();
	}

	void Start() {
		//updateFuel();
		//updateEnergy();
		//updateIntegrity();
	}

	public void updateFuel() {
		fuelText.text = "Fuel: " + floatToString(playerShipController.fuelPercent) + "% (" + floatToString(playerShipController.Fuel) + ")";
	}

	public void updateCargoFullness() {
		cargoFullness.text = "Cargo: " + (int)(playerShipCargo.TotalQuantity) + " / " +  (int)(playerShipCargo.Capacity);
	}

	public void updateEnergy() {
		energyText.text = "Energy: " + floatToString(playerShipController.energyPercent) + "% (" + floatToString(playerShipController.Energy) + ")";
	}

	public void updateCredits() {
		creditsText.text = "Credits: # " + floatToString(PlayerManager.Instance.Wallet.Credits);
	}

	public void updateIntegrity() {		
		string integrity = floatToString(playerShipController.gameObject.GetComponent<Damageable>().hullIntegrity);
		string hull = floatToString(playerShipController.gameObject.GetComponent<Damageable>().hull);
		integrityText.text = "Integrity: " + integrity + "% (" + hull + ")";
	}

	public void updateCargoPanel() {		
		foreach (Transform child in cargoPanel.transform)
			Destroy(child.gameObject);
		int pos = 0;
		foreach (Shushao.CargoContainer item in playerShipCargo.containers) {			
			addContainerToCargoPanel(cargoPanel, item, pos++);		
		}
	}

	IEnumerator cShowMessage(string text, float duration) {
		message.text = text;
		yield return new WaitForSeconds(duration);
		message.text = "";
		yield break;
	}

	public void ShowMessage(string text, float duration) {
		StartCoroutine(cShowMessage(text, duration));	
	}

	public void ShowMessage(string text) {
		StartCoroutine(cShowMessage(text, 3.0f));	
	}


	void addContainerToCargoPanel(GameObject panel, Shushao.CargoContainer container, int position) {
		GameObject item = Instantiate(Resources.Load("Prefabs/UIItem")) as GameObject;
		float ypos = position * item.GetComponent<RectTransform>().rect.height;
		item.transform.position = new Vector2(item.transform.position.x, ypos);
		item.name = container.contentType.ToString();
		item.transform.SetParent(panel.transform, false);
		item.GetComponent<UIItem>().setContainer(container);
		item.SetActive(true);
	}

	string floatToString(float num) {
		return string.Format("{0:0.00}", num);
	}

}
