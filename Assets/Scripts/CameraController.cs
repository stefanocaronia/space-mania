using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    
    public ShipController followed;
    public float cameraSpeed = 1.0f;

    private Vector3 cameraOffset;
    private Vector3 lastPosition;

    // Use this for initialization
    void Start () {
        cameraOffset = followed.gameObject.transform.position - transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        Vector3 newPosition = followed.gameObject.transform.position - cameraOffset;
        if (newPosition != lastPosition) {
            lastPosition = newPosition;
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * cameraSpeed);
        }       
    }
}
