using FishNet.Component.Transforming;
using UnityEngine;
using FishNet.Object;

public struct CharacterItemInput {
	public Quaternion Rotation;
	public bool Pickup;
	public bool Drop;
	public bool Shoot;
	public bool Aim;
}

public class PlayerItemController : NetworkBehaviour {

	[Header("Basic information")]
	[SerializeField] private Weapon heldWeapon;
	[SerializeField] private Transform holdPoint;
	[SerializeField] private Transform aimPoint;
	[SerializeField] private Transform playerCamera;
	[Header("Attributes")]
	[SerializeField] private float pickUpDistance = 2.5f;
	[SerializeField] private float timeToAim = 0.4f;
	
	public Transform GetPlayerCamera() => playerCamera;
	public Transform GetHoldPoint() => holdPoint;
	
	private bool _holdingWeapon;

	private Quaternion _requestedRotation;
	private bool _requestedPickup;
	private bool _requestedDrop;
	private bool _requestedShoot;
	private bool _requestedAim;

	public void Initialize(/*Transform cameraTarget*/) {
		/*this.cameraTarget = cameraTarget;*/
		_holdingWeapon = false;
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit,
			pickUpDistance);
		Gizmos.DrawLine(playerCamera.position, playerCamera.position + playerCamera.forward * pickUpDistance);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(hit.point, 0.1f);
	}

	public void UpdateInput(CharacterItemInput itemInput) {
		_requestedRotation = itemInput.Rotation;
		_requestedPickup = itemInput.Pickup;
		_requestedDrop = itemInput.Drop;
		_requestedShoot = itemInput.Shoot;
		_requestedAim = itemInput.Aim;
	}

	void Update() {
		if (base.IsOwner) {
			UpdateWeaponAim();
		
			//Pick up
			if (_requestedPickup) {
				Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit,
					pickUpDistance);

				PickableWeapon item = hit.collider.gameObject.GetComponent<PickableWeapon>();
				if (item != null) {
					PickupItem(item.GetComponent<NetworkObject>());
				}
			}
			//Drop
			else if(_requestedDrop) {
				DropItem();
			}
		}
	}

	private void UpdateWeaponAim() {
		if(!_holdingWeapon)
			return;

		if (heldWeapon is not Firearm firearm)
			return;
		
		if (_requestedAim)
			firearm.Aim(aimPoint.localPosition);
		else
			firearm.StopAiming(holdPoint.localPosition);
		
		/*weaponPoint.localPosition = Vector3.SmoothDamp(
			weaponPoint.localPosition,
			targetPosition,
			ref heldWeapon.weaponVelocity,
			timeToAim);*/
		//weaponPoint.localPosition = Vector3.Lerp(weaponPoint.localPosition, targetPosition, timeToAim);
	}

	[ServerRpc(RequireOwnership = false)]
	private void PickupItem(NetworkObject networkWeapon) { 
		//Change ownership
		networkWeapon.GiveOwnership(base.Owner);
		AttachItemObserverRpc(networkWeapon.ObjectId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DropItem() {
		NetworkObject networkWeapon = heldWeapon.GetComponent<NetworkObject>();
		//Change ownership
		networkWeapon.RemoveOwnership();
		DetachItemObserverRpc(networkWeapon.ObjectId);
	}

	[ObserversRpc(BufferLast = true)]
	private void AttachItemObserverRpc(int objectId) {
		
		NetworkObject item = ClientManager.Objects.Spawned[objectId];
		
		if(_holdingWeapon)
			DropItem();
		heldWeapon = item.GetComponent<PickableWeapon>().PickUp(this);
		_holdingWeapon = true;
	}

	[ObserversRpc(BufferLast = true)]
	private void DetachItemObserverRpc(int objectId) {
		
		NetworkObject item = ClientManager.Objects.Spawned[objectId];
		
		if (!_holdingWeapon)
			return;
		heldWeapon.Drop(holdPoint.position);
		heldWeapon = null;
		_holdingWeapon = false;
	}
	
}
