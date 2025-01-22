using FishNet.Object;
using UnityEngine;

using static WeaponSlot;

public class PlayerInventory : NetworkBehaviour {

	[SerializeField] private Weapon fists;
	[SerializeField] private MeleeWeapon meleeSlot = null;
	[SerializeField] private Firearm sidearmSlot = null;
	[SerializeField] private Firearm rifleSlot = null;
	[SerializeField] private Weapon throwableSlot = null;
	[SerializeField] private Weapon miscSlot = null;
	private Weapon[] weaponSlots;

	[SerializeField] private WeaponSlot currentSlot = Fists;

	private void Awake() {
		weaponSlots = new Weapon[6]{
			fists,
			meleeSlot,
			sidearmSlot,
			rifleSlot,
			throwableSlot,
			miscSlot
		};
	}

	public Weapon GetWeaponAtSlot(WeaponSlot slot) {
		return weaponSlots[(int)slot];
	}

	public Weapon GetCurrentWeapon() {
		return weaponSlots[(int)currentSlot];
	}
	
}