using UnityEngine;
using Shushao;

[RequireComponent(typeof(Rigidbody2D))]
public class Damageable : ScriptComponent {

	public float hullCapacity = 100.0f;
	public float hull = 100.0f;
	[HideInInspector]
	public float hullIntegrity = 100.0f;
	// percent
	[Range(0.0f, 1.0f)]
	public float hardiness = 0.4f;

	public bool exploding = false;

	public bool DamageFromCollisions = true;

	void Awake() {
		InitScriptComponent();

	}

	void Start() {
		if (isPlayerShip)
			GameManager.Instance.UI.updateIntegrity();
	}

	public void TakeDamage(float damage, GameObject shooter) {

		// Debug.Log(name + " shooted by " + shooter.name);

		if (isShip && SHIP.Shield) {
			if (Animator != null)
				Animator.SetTrigger("isShieldHit");
			SHIP.consumeEnergy(damage);
			return;	
		} else if (Animator != null) {
			Animator.SetTrigger("isHit");
		}

		damage /= Mathf.Abs(hardiness) + 1;
		hull -= damage;
		hull = Mathf.Clamp(hull, 0.0f, hullCapacity);

		hullIntegrity = (100 / hullCapacity) * hull;

		if (isPlayerShip)
			GameManager.Instance.UI.updateIntegrity();

		if (hull <= 0) {
			Explode(shooter);
		}

		if (isAI && (AI.Template.Behaviour == AIBehaviour.DEFENSIVE || AI.Template.Behaviour == AIBehaviour.AGGRESSIVE)) {
			AI.Target = shooter;
			AI.Action = AIAction.ATTACK;
		}
	}

	public void Repair(float amount) {
		hull += amount;
		hull = Mathf.Clamp(hull, 0.0f, hullCapacity);
		hullIntegrity = (100 / hullCapacity) * hull;
		if (isPlayerShip)
			GameManager.Instance.UI.updateIntegrity();
	}

	public void FullRepair() {
		hull = hullCapacity;
		hullIntegrity = (100 / hullCapacity) * hull;
		if (isPlayerShip)
			GameManager.Instance.UI.updateIntegrity();
	}

	public void Explode(GameObject killer) {

		if (transform.position != Vector3.zero) {	
			if (isShip)
				WorldController.Instance.ExplosionPool.Get(transform.position, transform.rotation);
			else if (isItem || isAsteroid || isWreck)
				WorldController.Instance.ExplosionLightPool.Get(transform.position, transform.rotation);
		}

		SendMessage("OnExplode", SendMessageOptions.DontRequireReceiver);

		exploding = true;

		if (killer != null && killer.GetComponent<ShipController>() != null) {

			if (GetComponent<AIController>() != null) {

				if (SHIP.Bounty > 0.0f) {
					if (killer.GetComponent<ShipController>().isPlayerShip) {
						float received = PlayerManager.Instance.Wallet.Receive(SHIP.Bounty);
						GameManager.Instance.UI.ShowMessage("You earned a bounty of " + received + " credits");
					} else { 
						killer.GetComponent<ShipController>().Wallet.Receive(SHIP.Bounty);
					}
					
				}

				if (SHIP.Wallet.Credits > 0.0f) {

					if (killer.GetComponent<ShipController>().isPlayerShip) {
						float stolen = SHIP.Wallet.Transfer(PlayerManager.Instance.Wallet);
						GameManager.Instance.UI.ShowMessage("You stole " + stolen + " credits");
					} else if (killer.GetComponent<ShipController>() != null) { 
							killer.GetComponent<ShipController>().Wallet.Transfer(killer.GetComponent<ShipController>().Wallet);
						}
				}
			}

			if (GetComponent<WreckController>() != null) {
				if (GetComponent<WreckController>().Wallet.Credits > 0.0f) {
					if (killer != null) {
						if (killer.GetComponent<ShipController>().isPlayerShip) {
							float stolen = GetComponent<WreckController>().Wallet.Transfer(PlayerManager.Instance.Wallet);
							GameManager.Instance.UI.ShowMessage("You stole " + stolen + " credits");
						} else { 
							GetComponent<WreckController>().Wallet.Transfer(killer.GetComponent<ShipController>().Wallet);
						}
					}
				}
			}
		}

//		if ((isShip || isWreck) && hasCargo) {
//			if (CARGO.containers.Count > 0) {
//				foreach (CargoContainer container in CARGO.containers) {
//					CARGO.generateFloatingCargo(container);
//				}
//			}
//		}

		Die();
	}
}
