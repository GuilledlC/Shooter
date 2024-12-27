using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour {
	
	[SerializeField] private Slider healthSlider;
	[SerializeField] private TMP_Text healthValue;
	
	public void UpdateHealth(float currentHealth, float maxHealth) {
		healthSlider.value = currentHealth / maxHealth;
		healthValue.text = currentHealth.ToString("0");
	}
	
}
