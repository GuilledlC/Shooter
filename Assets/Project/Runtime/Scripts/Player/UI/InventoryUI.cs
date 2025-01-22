using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {

	[SerializeField] private Image icon;
	[SerializeField] private TMP_Text name;

	public void UpdateWeapon(Weapon weapon) {
		if (weapon == null) {
			icon.color = new Color(icon.color.r, icon.color.b, icon.color.g, 0);
			icon.sprite = null;
			name.text = null;
		} else {
			icon.color = new Color(icon.color.r, icon.color.b, icon.color.g, 1);
			icon.sprite = weapon.GetWeaponIcon();
			name.text = weapon.GetWeaponName();
		}
	}
	
}