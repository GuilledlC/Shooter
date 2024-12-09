using System;
using FishNet.Component.Transforming;
using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;

public enum WeaponSlot {
	Melee,
	Sidearm,
	Rifle,
	Throwable,
	Special
}

public abstract class Weapon : NetworkBehaviour {

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

	public void Aim(Vector3 aimPoint) {
		transform.localPosition = aimPoint;
	}

	public void StopAiming(Vector3 holdPoint) {
		transform.localPosition = holdPoint;
	}
	
	public virtual void Initialize(PlayerItemController player) {
		
		if(base.IsOwner)
			//Set it in teh gun layer so it stays on top of objects
			SetLayerRecursively(gameObject, LayerMask.NameToLayer("GunLayer"));

		enabled = true;
		transform.SetParent(player.GetPlayerCamera());
		transform.localPosition = player.GetHoldPoint().localPosition;
		transform.localRotation = quaternion.identity;
	}

	public virtual void Deactivate() {
		enabled = false;
		transform.SetParent(null);
	}

	public virtual PickableWeapon Drop(Vector3 position) {
		Deactivate();
		transform.SetParent(null);
		transform.position = position;
		pickableWeapon.Initialize();
		return pickableWeapon;
	}
	
	
	//DO THIS IN A SINGLETON OR SOMETHING, IT EXISTS ALSO IN PickableWeapon.cs
	public void SetLayerRecursively(GameObject obj, int newLayer) {
		// Update the layer of the object itself
		obj.layer = newLayer;

		// Update all children
		foreach (Transform child in obj.transform)
			SetLayerRecursively(child.gameObject, newLayer);
	}
	
}