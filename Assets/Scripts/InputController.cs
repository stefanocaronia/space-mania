using UnityEngine;
using System.Collections;
using Shushao;

public class InputController : ScriptComponent {

	float angle = 90.0f;
	float thrustForward = 0.0f;
	float shieldUp = 0.0f;
	bool fire = false;
	float horizontal = 0.0f;
	float vertical = 0.0f;
	float rotateX = 0.0f;
	float rotateY = 0.0f;

	bool release = false;
	bool action = false;

	public enum InputMode {
		ANGLE,
		ROTDIR,
		MOUSE
	}

	public InputMode MODE;

	void Awake() {
		initScriptComponent();
		Cursor.visible = false;
		MODE = InputMode.ANGLE;
	}

	// Update is called once per frame
	void Update() {

		thrustForward = Input.GetAxis("Thrust Forward");
		shieldUp = Input.GetAxisRaw("Shield");
		fire = Input.GetButton("Fire1");
		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		action = Input.GetButton("Action");
		release = Input.GetButton("Right Bumper");

		rotateX = Input.GetAxis("Rotate X");
		rotateY = Input.GetAxis("Rotate Y");

		if (MODE == InputMode.MOUSE) {
			
			if (Input.GetAxis("Mouse X") > 0.02f || Input.GetAxis("Mouse Y") > 0.02f) {
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = -10.0f;
				mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
				SHIP.LookAt(mousePosition);
			} 
		
		} else if (MODE == InputMode.ROTDIR) {
			
			SHIP.Rotate(-horizontal);
			SHIP.thrustForward(Mathf.Max(thrustForward, vertical));

		} else if (MODE == InputMode.ANGLE) {

			if (horizontal != 0.0f || vertical != 0.0f) {
				angle = Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg;
			} 
			SHIP.setAngle(Utility.joy2objRot(angle));	
			SHIP.thrustForward(thrustForward);
		}

		SHIP.SetShield(shieldUp > 0.0f);
		SHIP.SetFirePressed(fire);

		if (release) {
			SHIP.gameObject.GetComponent<Cargo>().jettisonContainer();
		}

		// richiesta di dock in station (pulsante Y)
		if (action && SHIP.NearToStation) {
			SHIP.requestDock();
		}
	}

}
