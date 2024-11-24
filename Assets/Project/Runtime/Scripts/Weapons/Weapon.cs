using System;
using UnityEngine;

public enum WeaponSlot {
	Melee,
	Sidearm,
	Rifle,
	Throwable,
	Special
}

public abstract class Weapon : MonoBehaviour {

	[Header("Information")]
	[SerializeField] private string weaponName;
	[SerializeField] private string weaponDescription;
	[SerializeField] private WeaponSlot weaponSlot;
	

	[Header("Visual")]
	[SerializeField] private Sprite weaponIcon;
	[SerializeField] private Sprite crosshair;

	[Header("Internal References")]
	[SerializeField] private GameObject weaponRoot;

	
}