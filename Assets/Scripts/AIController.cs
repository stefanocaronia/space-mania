using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Shushao;
using System.Linq;


public class AIController : ScriptComponent {
	
	Radar RADAR;

	[HideInInspector]
	public PathFinder PF;

	[HideInInspector]
	public bool Docking { get; set; }

	Vector3 direction;

	public GameObject Target;

	const float loopDelay = 0.2f;

	public AITemplate Template;

	const float _maxFollowDistance = 8.0f;

	public AIAction action = AIAction.STILL;
	public AIAction Action {
		get {
			return action;
		}
		set {
			action = value;
			if (!Utility.IsInGame(this.gameObject)) return;
			switch (action) {
				case AIAction.PATROL:
					StopAllCoroutines();
					StartCoroutine("cPatrol");
					break;

				case AIAction.GATHER:
					StopAllCoroutines();
					StartCoroutine("cGather");
					break;

				case AIAction.MINE:
					StopAllCoroutines();
					StartCoroutine("cMine");
					break;

				case AIAction.DOCK:
					StopAllCoroutines();
					StartCoroutine("cDock");
					break;

				case AIAction.STILL:
					StopAllCoroutines();
					stop();
					break;

				case AIAction.ATTACK:
					StopAllCoroutines();
					StartCoroutine("cAttack");
					break;
			}				
		}	
	}

	// Use this for initialization
	void Awake() {		
		initScriptComponent();

		Template = AIConfiguration.AITemplates[SHIP.Type];

		RADAR = GetComponent<Radar>();
		PF = new PathFinder(gameObject);

		if (Template.Avoid != null && Template.Avoid.Length > 0) {
			PF.Obstacles = Template.Avoid.ToList();
		}
	}

	void Start() {		
		RADAR.scanRepeating();
	}

	IEnumerator cPatrol() {

		RADAR.Enable();
		SHIP.SetFirePressed(false);

		List<Collider2D> results = new List<Collider2D>();
		
		while (action == AIAction.PATROL) {	

			if (PF.destinationSaved) {
				PF.restoreDestination(); 
			} else {
				PF.setRandomDestination();
			}

			while (!PF.atDestination) {

				if (Template.Goal == AIGoal.SELL && CARGO.isFull()) {
					Action = AIAction.DOCK;
					yield break;
				}

				if (Template.Desires.Length > 0) {
					RADAR.FindItems(Template.Desires, ref results);
					if (results.Count(isPickable) > 0 ) {
						PF.saveDestination();
						Action = AIAction.GATHER;
						yield break;
					}
				}

				if (Template.Targets.Contains(EntityType.Asteroid)) {
					if (RADAR.FindAsteroids(ref results) > 0) {
						PF.saveDestination();
						Action = AIAction.MINE;
						yield break;
					}
				}

				if (Template.Targets.Intersect(Tables.ShipTypes).Any()) {					
					if (RADAR.FindWithTag(Template.TargetsTags, Template.Goal == AIGoal.SELL, ref results) > 0) {
						PF.saveDestination();
						if (results[0] != null) {
							Target = results[0].gameObject;
							Action = AIAction.ATTACK;
							yield break;
						}
					}
				}

				moveTowardsDestination();

				yield return new WaitForSeconds(loopDelay);
			}

		}

		yield break;
	}

	IEnumerator cGather() {

		List<Collider2D> results = new List<Collider2D>();
		
		while (action == AIAction.GATHER) {

			if (CARGO.isFull()) {
				Action = AIAction.PATROL;
				yield break;
			}

			if (Template.Desires.Length == 0 ) {
				Action = AIAction.PATROL;
				yield break;			
			}
				
			if (RADAR.FindItems(Template.Desires, ref results) == 0) {
				Action = AIAction.PATROL;
				yield break;
			}

			RADAR.Disable();

			foreach (Collider2D item in results) {
				
				if (!isPickable(item)) continue;

				//PF.Destination = item.transform.position;

				PF.bindTarget(item.gameObject);

				while (!PF.atDestination) {

					if (item == null) break;

					PF.Destination = item.transform.position;
					moveTowardsDestination();

					yield return new WaitForSeconds(loopDelay);

				}

				PF.unbindTarget();

				if (CARGO.isFull()) {
					RADAR.Enable();
					Action = AIAction.DOCK;
					yield break;
				}
			}

			RADAR.Enable();
			Action = AIAction.PATROL;
			yield break;
		}

		RADAR.Enable();
		Action = AIAction.PATROL;
		yield break;
	}

	IEnumerator cDock() {

		GameObject nearestStation = findNearestStation();
		StationController stationController = nearestStation.GetComponent<StationController>();

		PF.Destination = nearestStation.transform.position;
		Docking = true;

		while (action == AIAction.DOCK) {
			
			while (PF.distanceFromDestination > 1.6f) {

				moveTowardsDestination();

				yield return new WaitForSeconds(loopDelay);
			}

			stop();

			if (SHIP.Type != EntityType.Ameba && !stationController.Occupied && !stationController.Operating && !stationController.Queued(this.gameObject)) {

				SHIP.requestDock();

			}

			yield return new WaitForSeconds(loopDelay * 6);
			
		}

		yield break;
	}

	IEnumerator cMine() {

		RADAR.Enable();

		GameObject nearestAsteroid;
		List<Collider2D> results = new List<Collider2D>();
		
		while (action == AIAction.MINE) {

			nearestAsteroid = getSmallestNearestAsteroid();

			if (nearestAsteroid == null) {
				Action = AIAction.PATROL;
				yield break;
			}

			PF.bindTarget(nearestAsteroid);

			RADAR.Disable();

			while (nearestAsteroid != null && nearestAsteroid.GetComponent<Damageable>().hull > 0) {
				
				if (!PF.aroundDestination(3.0f))
					moveTowardsDestination();
				else {
					stop();
					SHIP.LookAt(PF.Destination);
				}

				if (IsFacingTargets(3.0f))
					SHIP.SetFirePressed(true);
				
				yield return new WaitForSeconds(loopDelay);
			}

			RADAR.Enable();

			SHIP.SetFirePressed(false);
			PF.unbindTarget();

			if (Template.Desires.Any()) {
				RADAR.FindItems(Template.Desires, ref results);
				if (results.Count(isPickable) > 0) {
					PF.saveDestination();
					Action = AIAction.GATHER;
					yield break;
				}
			}

			yield return new WaitForSeconds(loopDelay);
		}

		Action = AIAction.PATROL;
		yield break;
	}

	IEnumerator cAttack() {

		List<Collider2D> results = new List<Collider2D>();

		RADAR.Disable();

		while (action == AIAction.ATTACK) {

			if (Target == null) {
				Action = AIAction.PATROL;
				yield break;
			}

			PF.bindTarget(Target);

			if (PF.Distance > _maxFollowDistance) {
				PF.unbindTarget();
				Action = AIAction.PATROL;
				yield break;
			}

			while (Target != null && Target.activeSelf && Target.GetComponent<SpriteRenderer>().enabled && Target.gameObject.GetComponent<Damageable>().hull > 0) {
								
				if (Template.Attack == AIAttack.RANGED) {
					
					if (!PF.aroundDestination(3.0f))
						moveTowardsDestination();
					else {
						stop();
						SHIP.LookAt(PF.Destination);
					}

					if (IsFacingTargets(3.0f))
						SHIP.SetFirePressed(true);
					
				} else if (Template.Attack == AIAttack.CONTACT) {
					
					if (!PF.atDestination)
						moveTowardsDestination();
					
				}

				yield return null;
			}

			Target = null;

			RADAR.Enable();

			SHIP.SetFirePressed(false);
			PF.unbindTarget();

			if (Template.Goal == AIGoal.SELL || Template.Goal == AIGoal.EAT && RADAR.FindItems(Template.Desires, ref results) > 0) {

				if (results.Count(isPickable) > 0 ) {
					PF.saveDestination();
					Action = AIAction.GATHER;
					yield break;

				} else {

					Action = AIAction.PATROL;
					yield break;
				}

			} else {
				
				Action = AIAction.PATROL;
				yield break;

			}
		}

		Action = AIAction.PATROL;
		yield break;
	}

	void moveTowardsDestination() {		
		direction = PF.getBestDirection();
		SHIP.LookAt(transform.position + (direction));
		move();
	}

	void move() {
		SHIP.thrustForward( PF.Slow ? 0.2f : 0.4f);
	}

	void stop() {
		SHIP.thrustForward(0.0f);
	}

	GameObject getSmallestNearestAsteroid () {
		GameObject result = null;
		List<Collider2D> results = new List<Collider2D>();
		AsteroidSize maxAsteroidType = AsteroidSize.NONE;
		int num = RADAR.FindAsteroids(ref results);
		if (num == 0) return null;
		foreach (Collider2D c in results) {
			if (c == null) continue;
			if (c.GetComponent<AsteroidController>().size > maxAsteroidType) {
				maxAsteroidType = c.GetComponent<AsteroidController>().size;
				result = c.gameObject;
			}			
		}
		return result;
	}

	bool isPickable(Collider2D item) {
		if (item == null) return false;
		if (item.GetComponent<Cargo>() == null) return false;
		ContentType type = item.GetComponent<Cargo>().ContentType;
		if (SHIP.Type != EntityType.Ameba) {
			if (type == ContentType.Cell && SHIP.energyPercent > 50.0f) return false;
			else if (type == ContentType.Fuel && SHIP.fuelPercent > 50.0f) return false;
			else if (type != ContentType.Cell && type != ContentType.Fuel && CARGO.isFull()) return false;
		}
		return true;
	}

	GameObject findNearestStation() {
		Entity nearest = WorldController.Instance.ENTITIES.findNearestByType(EntityType.Station, transform.position);
		return nearest.GameObject;
	}

	public bool Facing(LayerMask layerMask, float distance) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * GetComponent<PolygonCollider2D>().bounds.size.magnitude, transform.up, distance, layerMask);
		return (hit.collider != null);
	}

	public bool Facing<T>(float distance) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * GetComponent<PolygonCollider2D>().bounds.size.magnitude, transform.up, distance);
		if (hit.collider == null) 
			return false;
		return (hit.collider.gameObject.GetComponent<T>() != null);
	}

	public bool Facing(string tag, float distance) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * GetComponent<PolygonCollider2D>().bounds.size.magnitude, transform.up, distance, LayerMask.GetMask("Default"));
		if (hit.collider == null) 
			return false;
		return hit.collider.CompareTag(tag);
	}

	public bool IsFacingTargets(float distance) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * GetComponent<PolygonCollider2D>().bounds.size.magnitude, transform.up, distance, LayerMask.GetMask("Default"));
		if (hit.collider == null) 
			return false;
		return (Template.TargetsTags.Contains(hit.collider.tag) || hit.collider.gameObject == Target);
	}

//	GameObject findPlayer() {
//		if (PlayerManager.Instance.Ship != null) return null;
//		float distanceFromPlayer = (PlayerManager.Instance.Ship.transform.position - transform.position).magnitude;
//		if (distanceFromPlayer <= radarRadius)
//			return PlayerManager.Instance.Ship;
//		else
//			return null;
//	}

}