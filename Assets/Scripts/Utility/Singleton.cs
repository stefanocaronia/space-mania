using UnityEngine;
using System;


namespace Shushao {
	
	public class Singleton<T> where T : new() {

		static readonly T instance = new T();

		public static T Instance {
			get {
				return instance;
			}
		}
	}
}


