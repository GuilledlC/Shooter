using UnityEngine;

public class CameraTarget : MonoBehaviour {

	public void UpdateLocalPosition(Vector3 localPosition) {
		transform.localPosition = localPosition;
	}

	public Vector3 GetLocalPosition() {
		return transform.localPosition;
	}
	
}
