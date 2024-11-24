using UnityEngine;

public class MeleeWeapon : Weapon {

	private WeaponSlot weaponSlot = WeaponSlot.Melee;

	public void Initialize() {
		weaponSlot = WeaponSlot.Melee;
	}
}