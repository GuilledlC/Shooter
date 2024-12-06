using FishNet.Component.Transforming;
using UnityEngine;
using FishNet.Object;

public struct CharacterItemInput {
	public Quaternion Rotation;
	public bool Pickup;
	public bool Drop;
}

public class PlayerItemController : NetworkBehaviour {

	[Header("Basic information")]
	[SerializeField] private Weapon heldWeapon;
	[SerializeField] private Transform gunPoint;
	[Header("Attributes")]
	[SerializeField] private float pickUpDistance = 2.5f;
	
	public Transform GetGunPoint() => gunPoint;
	
	private Transform cameraTransform;

	private Quaternion _requestedRotation;
	private bool _requestedPickup;
	private bool _requestedDrop;

	public void Initialize(Transform cameraTransform) {
		this.cameraTransform = cameraTransform;
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
			pickUpDistance);
		Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * pickUpDistance);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(hit.point, 0.1f);
	}

	public void UpdateInput(CharacterItemInput itemInput) {
		_requestedRotation = itemInput.Rotation;
		_requestedPickup = itemInput.Pickup;
		_requestedDrop = itemInput.Drop;
	}

	void Update() {
		
		UpdateRotation();
		
		//Pick up
		if (_requestedPickup) {
			Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
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

	private void UpdateRotation() {
		if(heldWeapon != null)
			heldWeapon.transform.rotation = _requestedRotation;
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
		
		if(heldWeapon != null)
			DropItem();
		heldWeapon = item.GetComponent<PickableWeapon>().PickUp(this);

		item.transform.SetParent(gunPoint);
		item.transform.localPosition = -heldWeapon.GetGripPoint().localPosition;
		item.transform.localRotation = Quaternion.identity;
	}

	[ObserversRpc(BufferLast = true)]
	private void DetachItemObserverRpc(int objectId) {
		
		NetworkObject item = ClientManager.Objects.Spawned[objectId];
		
		if (heldWeapon == null)
			return;
		heldWeapon.Drop(gunPoint.position);
		heldWeapon = null;
		
		item.transform.SetParent(null);
	}
	
}
