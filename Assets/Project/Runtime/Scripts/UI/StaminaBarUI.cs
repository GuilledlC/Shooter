using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour {

	[SerializeField] private Slider staminaSlider;
	[SerializeField] private TMP_Text staminaValue;
	[SerializeField] private TMP_Text speed;

	public void UpdateStamina(float currentStamina, float maxStamina) {
		staminaSlider.value = currentStamina / maxStamina;
		staminaValue.text = currentStamina.ToString("0.00");
	}
}
