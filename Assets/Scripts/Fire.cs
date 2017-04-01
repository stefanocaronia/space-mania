using UnityEngine;
using System.Collections;
using Shushao;

public class Fire : ScriptComponent, iPoolable {

	public float power = 20.0f;
	public float force = 10.0f;

	public GameObject shooter;
	public float additionalSpeed = 0.0f;

	private const float lifeSpan = 2.0f;
	public float born = 0.0f;

	#region iPoolable implementation

	public void Initialize() {
		born = Time.time;
	}

	public void Deactivate() {
	}

	public Pool Source { get;set; }

	#endregion

	void Awake(){		
		initScriptComponent();
	}

	
	// Update is called once per frame
	void Update () {
		if (Time.time - born > lifeSpan) 
			die();
	}


	public void shot() {
		born = Time.time;
		RIGIDBODY.AddForce(transform.up * force, ForceMode2D.Force);
	}

	void OnTriggerEnter2D (Collider2D other) {
		
		if (other.gameObject == shooter)
			return;	

		if (other.isTrigger)
			return;

		if (other.GetComponent<Damageable>() != null) {
			Damageable otherController = other.GetComponent<Damageable> ();
			otherController.TakeDamage(power, this.shooter);
		}
		hit();
	}

	public void hit() {
		if (transform.position != Vector3.zero) {
			WorldController.Instance.SparksPool.Get(transform.position, transform.rotation);
		}
		die();
	}
}
