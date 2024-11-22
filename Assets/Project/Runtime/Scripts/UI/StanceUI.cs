using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StanceUI : MonoBehaviour {
	
	[SerializeField] private TMP_Text stance;
	
	public void UpdateStance(Stance stance, bool grounded) {
		this.stance.text = stance.ToString();
		/*if (!grounded)
			this.stance.text = "Flight";*/
	}

}
