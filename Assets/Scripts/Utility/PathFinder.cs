using UnityEngine;
using Shushao;
using System.Collections.Generic;

public class PathFinder {

	bool debug = false;
	
	GameObject agent;
	public List<string> Obstacles;
    public List<string> Allowed = new List<string>();
    public LayerMask Layers;

	const int anglesSensitivity = 8;
	const int obstaclesAngle = 56;
	const float minDistanceFromObstacles = 1.0f;
	public Vector3 savedDestination = Vector3.zero;
	public bool destinationSaved;

	GameObject bindedTarget = null;

	public PathFinder(GameObject go, List<string> obstacles, LayerMask layers) {
		agent = go;
		Obstacles = obstacles;
		Layers = layers;
	}

	public PathFinder(GameObject go) {
		agent = go;
		Obstacles = new List<string> {
			"Asteroid",
			"Station",
			"Miner",
			"Transport",
			"Player",
			"Pirate"
		};
		Layers = LayerMask.GetMask("Default");
	}

	private Vector3 _destination;
	public Vector3 Destination { 
		get { 
			if (bindedTarget != null)
				return bindedTarget.transform.position;
			else
				return _destination;
		}
		set {
			_destination = value;
		}
	}

	public bool Slow { get; set; }	

	public bool AtDestination {
		get {
			return (Destination == agent.transform.position);
        }
	}
    
    public bool AroundDestination(float radius) {
		return (Destination - agent.transform.position).magnitude <= radius;
	}

	public float Distance {
		get { 
			return (Destination - agent.transform.position).magnitude; 
		}
	}

	bool IsObstacle(Collider2D collider) {
		if (collider == null || !Utility.ColliderIsInGame(collider)) 
			return false;
		return (Obstacles.Contains(collider.tag) && !Allowed.Contains(collider.tag));
	}
    
	public Vector3 getBestDirection() {
		
		RaycastHit2D hit;
		float minDistance = 9999999.0f;
		float DistanceFromDestination;
		int result = 0;
		Vector3 dir;
		List<int> excluded = new List<int>();
		float size = agent.GetComponent<PolygonCollider2D>().bounds.size.magnitude * 0.6f;

		if (debug) Debug.DrawLine(agent.transform.position, Destination, Color.magenta);

		Slow = false;

		if (debug) Debug.DrawRay(agent.transform.position + (agent.transform.right * 0.2f), agent.transform.up * minDistanceFromObstacles*2, Color.red);
		hit = Physics2D.Raycast(agent.transform.position + (agent.transform.right * 0.2f), agent.transform.up, minDistanceFromObstacles *2, Layers);
		if (hit.collider != null && IsObstacle(hit.collider) && !(agent.GetComponent<AIController>().Docking && hit.collider.CompareTag("Station"))) {			
			Slow = true;
		}

		if (debug) Debug.DrawRay(agent.transform.position - (agent.transform.right * 0.2f), agent.transform.up * minDistanceFromObstacles *2, Color.red);
		hit = Physics2D.Raycast(agent.transform.position - (agent.transform.right * 0.2f), agent.transform.up, minDistanceFromObstacles*2, Layers);

		if (hit.collider != null && IsObstacle(hit.collider) && !(agent.GetComponent<AIController>().Docking && hit.collider.CompareTag("Station"))) {			
			Slow = true;
		}

		for (int angle = 0; angle < 360; angle += anglesSensitivity) {
			
			if (excluded.Contains(angle))
				continue;
			
			dir = Utility.angle2Direction(angle);
			hit = Physics2D.Raycast(agent.transform.position + dir * size, dir, minDistanceFromObstacles, Layers);
		
			//if (hit.collider != null && hit.collider.gameObject != agent.gameObject && isObstacle(hit.collider) && !(agent.GetComponent<AIController>().Docking && hit.collider.CompareTag("Station"))) {
			if (hit.collider != null && hit.collider.gameObject != agent.gameObject && IsObstacle(hit.collider)) {
				if (debug) Debug.DrawRay(agent.transform.position + dir * size, Utility.angle2Direction(angle) * minDistanceFromObstacles, Color.red);
				excluded.Add(angle);

				for (int a = angle - obstaclesAngle; a <= angle + obstaclesAngle; a += anglesSensitivity) {
					if (excluded.Contains(a)) continue;
					if (debug) Debug.DrawRay(agent.transform.position + Utility.angle2Direction(a) * size, Utility.angle2Direction(a) * minDistanceFromObstacles, Color.red);
					excluded.Add(Utility.ClampAngle(a));
				}
			}
		}

		for (int angle = 0; angle < 360; angle += anglesSensitivity) {
			if (excluded.Contains(angle))
				continue;
			dir = Utility.angle2Direction(angle);
			if (debug) Debug.DrawRay(agent.transform.position + dir * size, dir * minDistanceFromObstacles, Color.blue);
			DistanceFromDestination = (Destination - (agent.transform.position + dir)).sqrMagnitude;
			if (DistanceFromDestination < minDistance) {
				minDistance = DistanceFromDestination;
				result = angle;
			}
		}

		if (debug) Debug.DrawRay(agent.transform.position, Utility.angle2Direction(result), Color.green);

		return Utility.angle2Direction(result);
	}

	public float DistanceFromDestination {
		get {
			return (Destination - agent.transform.position).magnitude;
		}
	}

	public Vector3 SetRandomDestination() {
		float x = Random.Range(-WorldController.Instance.WIDTH, WorldController.Instance.WIDTH);
		float y = Random.Range(-WorldController.Instance.HEIGHT, WorldController.Instance.HEIGHT);
		Destination = new Vector3(x, y, 0.0f);
		return Destination;
	}

	public void BindTarget (GameObject target) {
		if (target != null)
			bindedTarget = target;
	}

	public void UnbindTarget () {
		bindedTarget = null;
		if (destinationSaved)
			RestoreDestination();
	}

	public void SaveDestination() {
		savedDestination = Destination;
		destinationSaved = true;
	}

	public void RestoreDestination() {
		Destination = savedDestination;
		savedDestination = Vector3.zero;
		destinationSaved = false;
	}

}
