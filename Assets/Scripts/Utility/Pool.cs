using System.Collections.Generic;
using UnityEngine;

namespace Shushao {

	public interface iPoolable {		
		Pool Source { get; set; }
		void Initialize();
		void Deactivate();
	}

	[System.Serializable]
	public class Pool : Stack<GameObject> {
		
		[SerializeField]
		private int size = 10;
		[SerializeField]
		private int expand = 4;
		[SerializeField]
		GameObject prefab = null;

		public Pool() {
		}

		public Pool (GameObject g, int s, int e) {
			size = s;
			expand = e;
			prefab = g;
		}

		public void Populate() {
			ExpandPoolPopulation(size);
		}

		void ExpandPoolPopulation(int amount) {
			for (int j = 0; j < amount; j++) {
				GameObject result = GameObject.Instantiate(prefab);
				result.GetComponent<iPoolable>().Source = this;
				Put(result);
			}
		}

		public GameObject Get() {
			if (Count <= 0)
				ExpandPoolPopulation(expand);
			GameObject result = Pop();
			result.SetActive(true);
			result.GetComponent<iPoolable>().Initialize();
			return result;
		}

		public GameObject Get(Vector3 position, Quaternion rotation) {
			GameObject result = Get();
			result.transform.position = position;
			result.transform.rotation = rotation;
			if (result.GetComponent<PolygonCollider2D>() != null)
				result.GetComponent<PolygonCollider2D>().enabled = true;
			if (result.GetComponent<BoxCollider2D>() != null) {
				result.GetComponent<BoxCollider2D>().enabled = true;
            }
            if (result.GetComponent<CircleCollider2D>() != null)
				result.GetComponent<CircleCollider2D>().enabled = true;
			return result;
		}

		public void Put(GameObject handled) {			
			handled.GetComponent<iPoolable>().Deactivate();
			handled.SetActive(false);
			if (handled.GetComponent<PolygonCollider2D>() != null)
				handled.GetComponent<PolygonCollider2D>().enabled = false;
			if (handled.GetComponent<BoxCollider2D>() != null)
				handled.GetComponent<BoxCollider2D>().enabled = false;
			if (handled.GetComponent<CircleCollider2D>() != null)
				handled.GetComponent<CircleCollider2D>().enabled = false;
			handled.transform.position = Vector3.zero;
			handled.transform.rotation = Quaternion.identity;
			PoolFolder.Park(handled);
			Push(handled);
		}
	}

}