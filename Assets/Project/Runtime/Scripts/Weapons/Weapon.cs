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
	[SerializeField] private PlayerItemController playerToFollow;

	public Transform GetGripPoint() => gripPoint;


	private void Update() {
		if (playerToFollow)
			this.transform.position = playerToFollow.GetGunPoint().position;
	}

	public virtual void Initialize(PlayerItemController player) {
		SetLayerRecursively(gameObject, LayerMask.NameToLayer("GunLayer"));

		enabled = true;
		playerToFollow = player;
	}

	public virtual void Deactivate() {
		enabled = false;
		playerToFollow = null;
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