using UnityEngine;
using System.Collections;
using Shushao;

public class Explosion : MonoBehaviour, iPoolable {
	
	#region iPoolable implementation

	public void Initialize() {
		destroy();
	}

	public void Deactivate() {
	}

	public Pool Source {
		get; set;
	}

	#endregion

	public void Start() {
		destroy();
	}

	private void destroy() {
		float delay = 2.0f;
		if (GetComponent<ParticleSystem>() != null) {
            delay = GetComponent<ParticleSystem>().main.duration * 2;
        }
        Invoke("die", delay);
	}

	public void die(){
		Source.Put(this.gameObject);
	}
}
