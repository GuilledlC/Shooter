using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateItem : MonoBehaviour {

	[Header("Setting")]
	public float rotateSpeed = 1;
	public bool clockwise = true;

	private float yRotation;
	
    // Start is called before the first frame update
    void Start() {
	    yRotation = 0;
	    transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update() {
	    if (!clockwise)
		    rotateSpeed *= -1;
	    yRotation += rotateSpeed/30;
	    transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
