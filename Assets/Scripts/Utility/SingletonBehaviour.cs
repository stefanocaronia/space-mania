using UnityEngine;

namespace Shushao {

	public class SingletonBehaviour<T> : MonoBehaviour where T: MonoBehaviour {

		private static T instance;

		public static T Instance {
			get {
				if (instance == null) {
					instance = (T)FindObjectOfType(typeof(T));
					//if (typeof(T)==System.Type.GetType("GameManager")) DontDestroyOnLoad(instance.gameObject);
				}
				return instance;
			}
		}
	}
}

