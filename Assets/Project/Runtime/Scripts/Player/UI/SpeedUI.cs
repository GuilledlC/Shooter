using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedUI : MonoBehaviour {
	
	[SerializeField] private TMP_Text speed;
	
	public void UpdateSpeed(float speed) {
		const float mpsToKmh = 3.6f;
		float kmh = speed * mpsToKmh;
		this.speed.text = kmh.ToString("0") + " km/h";
	}

}
