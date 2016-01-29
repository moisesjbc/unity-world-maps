using UnityEngine;
using System.Collections;

public class AirCamera : MonoBehaviour {
	public float speed = 0.0f;
	public float minSpeed = -5.0f;
	public float maxSpeed = 5.0f;
	public float speedStep = 0.1f;
	public float ROTATION_SENSITIVITY = 5.0f;

	// Update is called once per frame
	void Update () {
	
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			Debug.Log ("Increasing speed");
			speed += speedStep;
		}else if(Input.GetAxis ("Mouse ScrollWheel") < 0){
			speed -= speedStep;
		}
			
		speed = Mathf.Clamp (speed, minSpeed, maxSpeed);

		Vector3 velocity = speed * Vector3.forward * Time.deltaTime;

		// Allow user to rotate the camera with alt + the mouse.
		if( Input.GetKey(KeyCode.LeftAlt) ){
			transform.Rotate ( ROTATION_SENSITIVITY * -Input.GetAxis ("Mouse Y"),
				ROTATION_SENSITIVITY * Input.GetAxis ("Mouse X"), 
				0.0f );
		}


		transform.Translate (velocity);
	}
}
