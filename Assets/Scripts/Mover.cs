using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class Mover : ScriptComponent {

	public bool rotateAtStart;
	public bool moveAtStart;

	protected float rotationSpeed;
	public float maxTorque = 10.2f;
	public float maxForce = 0.1f;
	public float maxSpeed = 1.5f;
	public float maxRotationSpeed = 30.0f;

	public Vector2 forcedVelocity = new Vector2(0.0f, 0.0f);


	void Awake() {
		InitScriptComponent();
		rotationSpeed = Random.Range(-0.6f, 0.6f);
	}

	void Start() {	
	
		if (RIGIDBODY.isKinematic) {

			if (rotateAtStart) {
				rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
			}

		} else {

			if (moveAtStart) {
				RIGIDBODY.AddForce(new Vector2(Random.Range(-maxForce, maxForce), Random.Range(-maxForce, maxForce)), ForceMode2D.Impulse);
			}

			if (rotateAtStart) {
				RIGIDBODY.AddTorque(Random.Range(-maxTorque, maxTorque));
			}
		}
	}

	public void forceMovement() {
		RIGIDBODY.velocity = forcedVelocity;
	}

	void Update() {

		if (RIGIDBODY.isKinematic) {
			
			RIGIDBODY.angularVelocity = rotationSpeed;

		} else {

			// limito la velocità massima
			if (RIGIDBODY.velocity.magnitude > maxSpeed) {
				RIGIDBODY.velocity = RIGIDBODY.velocity.normalized * maxSpeed;
			}

			// limito rotazione massima
			if (Mathf.Abs(RIGIDBODY.angularVelocity) > maxRotationSpeed) {
				RIGIDBODY.angularVelocity = Mathf.Sign(RIGIDBODY.angularVelocity) * maxRotationSpeed;
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
	
//		switch (other.gameObject.tag) {
//		case "Limits":
//			//BoxCollider2D collider = other.collider.
//		}
	}
}
