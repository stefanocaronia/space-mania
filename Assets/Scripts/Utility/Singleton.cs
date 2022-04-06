namespace Shushao {
	
	public class Singleton<T> where T : new() {

		private static readonly T instance = new T();

		public static T Instance {
			get {
				return instance;
			}
		}
	}
}


