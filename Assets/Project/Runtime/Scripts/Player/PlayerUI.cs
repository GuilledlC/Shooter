using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

	[SerializeField] private StaminaBarUI staminaBar;
	[SerializeField] private StanceUI stance;
	[SerializeField] private SpeedUI speed;
	
	public void UpdateUI(PlayerCharacter playerCharacter) {
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
