using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float Angle;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float speed = 20.0f;
		transform.Rotate(-Vector3.forward, Input.GetAxis("Rotate X") * speed * Time.deltaTime);
	}

	public void SetAngle(float angle) {
		
	}
}
