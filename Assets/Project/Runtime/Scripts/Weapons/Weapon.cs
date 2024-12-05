using System;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;

public enum WeaponSlot {
	Melee,
	Sidearm,
	Rifle,
	Throwable,
	Special
}

/*[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]*/
public abstract class Weapon : MonoBehaviour {

	[Header("Item")]
	[SerializeField] private PickableWeapon pickableWeapon;
	
	[Header("Information")]
	[SerializeField] private string weaponName;
	[SerializeField] private string weaponDescription;
	[SerializeField] private WeaponSlot weaponSlot;

	[Header("Visual")]
	[SerializeField] private Sprite weaponIcon;
	[SerializeField] private Sprite crosshair;

	[Header("Internal References")]
	[SerializeField] private GameObject weaponRoot;
	[SerializeField] private Transform gripPoint;

	public Transform GetGripPoint() => gripPoint;
	
	public virtual void Initialize() {
		Debug.Log("Weapon init");
		enabled = true;
	}

	public virtual void Deactivate() {
		Debug.Log("Weapon deactivate");
		enabled = false;
	}

	public virtual PickableWeapon Drop(Vector3 position) {
		Deactivate();
		transform.SetParent(null);
		transform.position = position;
		pickableWeapon.Initialize();
		return pickableWeapon;
	}
	
}