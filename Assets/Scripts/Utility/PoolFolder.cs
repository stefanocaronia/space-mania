using UnityEngine;

public class PoolFolder : MonoBehaviour {
	public static PoolFolder instance;

	void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	public static void Park(GameObject poolable) {
		if (poolable == null || instance == null) 
			return;
		poolable.transform.parent = instance.transform;
	}
}
