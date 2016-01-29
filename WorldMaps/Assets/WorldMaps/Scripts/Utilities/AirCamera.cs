using UnityEngine;
using System.Collections;

public class AirCamera : MonoBehaviour {
	public float speed = 0.0f;
	public float minSpeed = -5.0f;
	public float maxSpeed = 5.0f;
	public float speedStep = 0.1f;
	public float ROTATION_SENSITIVITY = 5.0f;
	private Vector3 initialPosition = Vector3.zero;


	void Awake(){
		initialPosition = transform.position;
		Cursor.lockState = CursorLockMode.Locked;
		GetComponent<Rigidbody> ().freezeRotation = true;
	}


	// Update is called once per frame
	void Update () {
	
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
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

		// Move the player forward with the given speed.
		GetComponent<Rigidbody>().MovePosition(transform.position + /*GetComponent<OVRCameraRig> ().centerEyeAnchor.rotation */
			(speed * Time.fixedDeltaTime * Vector3.forward));
		
		transform.Translate (velocity);
	}
		

	void OnCollisionEnter()
	{
		speed = 0.0f;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
	}
}
