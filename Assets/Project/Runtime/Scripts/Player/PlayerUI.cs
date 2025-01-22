using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

	[SerializeField] private Canvas canvas;
	[SerializeField] private HealthBarUI healthBar;
	[SerializeField] private StaminaBarUI staminaBar;
	[SerializeField] private StanceUI stance;
	[SerializeField] private SpeedUI speed;
	[SerializeField] private InventoryUI inventory;

	private bool _requestPause = false;

	public void UpdateInput(bool pause) {
		if(pause)
			_requestPause = !_requestPause;
	}

	public void Initialize(PlayerHealth playerHealth, PlayerItemController playerItemController) {
		playerHealth.OnHealthChanged += healthBar.UpdateHealth;
		playerItemController.OnWeaponChanged += inventory.UpdateWeapon;
	}
	
	public void UpdateUI(PlayerCharacter playerCharacter) {
		canvas.enabled = !_requestPause;
		
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
