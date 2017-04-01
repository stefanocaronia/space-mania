using UnityEngine;
using Shushao;

public class GameManager : SingletonBehaviour<GameManager> {

	public GameStates gameState;
	[HideInInspector]
	public Canvas UICanvas;
	[HideInInspector]
	public UIController UI;

	[HideInInspector]
	public ShipController playerShipController;

	public uint level = 1;
	public uint difficulty = 1;

	public bool Simulation;

	void Awake() {
		
		gameState = GameStates.RUNNING;

		UI = FindObjectOfType<UIController>();
		UICanvas = UI.GetComponent<Canvas>();

	}

	// Use this for initialization
	void Start() {
		
		WorldController.Instance.generate();

		UI.ShowMessage("START!");

	}
}
