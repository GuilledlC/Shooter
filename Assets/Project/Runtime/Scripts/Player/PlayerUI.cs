using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

	[SerializeField] private Canvas canvas;
	[SerializeField] private HealthBarUI healthBar;
	[SerializeField] private StaminaBarUI staminaBar;
	[SerializeField] private StanceUI stance;
	[SerializeField] private SpeedUI speed;

	private bool _requestPause = false;

	public void UpdateInput(bool pause) {
		if(pause)
			_requestPause = !_requestPause;
	}

	public void UpdateUI(PlayerHealth playerHealth, PlayerCharacter playerCharacter) {
		canvas.enabled = !_requestPause;
		
		healthBar.UpdateHealth(
			playerHealth.GetCurrentHealth(),
			playerHealth.GetMaxHealth());
		staminaBar.UpdateStamina(
			playerCharacter.GetCharacterState().Stamina,
			playerCharacter.GetMaxStamina());
		stance.UpdateStance(
			playerCharacter.GetCharacterState().Stance,
			playerCharacter.GetCharacterState().Grounded);
		speed.UpdateSpeed(
			playerCharacter.GetCharacterState().Speed);
	}
}
