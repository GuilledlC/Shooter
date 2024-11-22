using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.GuilledlC.Shooter {

	public class PlayerCamera : MonoBehaviour {

		public float Sensitivity;

		public GameObject Player;
		
		private float xRotation;
		private float yRotation;
		
		void Start() {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			
			//Rotate camera
			transform.rotation = Quaternion.Euler(0, 0, 0);
			//Rotate player
			Player.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		
		void Update() {
	    
			//Get input
			float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * Sensitivity;
			float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * Sensitivity;

			yRotation += mouseX;
			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, -90f, 90f);
	    
			//Rotate camera
			transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
			//Rotate player
			Player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
		}
	}
}
