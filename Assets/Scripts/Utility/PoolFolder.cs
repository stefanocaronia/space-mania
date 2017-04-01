using UnityEngine;

public class PoolFolder : MonoBehaviour {
	public static PoolFolder instance;

	void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);

		// instance.name = "POOL";
	}

	public static void Park(GameObject poolable) {
		if (poolable == null) 
			return;

		poolable.transform.parent = instance.transform;
	}
}
