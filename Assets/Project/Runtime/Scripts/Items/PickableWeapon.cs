using System.Collections;
using FishNet.Component.Transforming;
using UnityEngine;
using FishNet.Object;

public class PickableWeapon : MonoBehaviour {

	[Header("Basic information")]
	[SerializeField] private Rigidbody rigidbody;
	[SerializeField] private MeshCollider collider;
	[SerializeField] private CapsuleCollider pickupRange;
	[SerializeField] protected Weapon weapon;

	private void Reset() {
		//https://docs.unity3d.com/6000.0/Documentation/Manual/choose-collision-detection-mode.html
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		
		collider = GetComponent<MeshCollider>();
		collider.convex = true;

		pickupRange = GetComponent<CapsuleCollider>();
		pickupRange.isTrigger = true;
	}

	protected virtual void Start() {
		Initialize();
		//Destroy(gameObject, 90); //THIS DOESNT WORK, THE TIMER DOESNT RESET
	}

	private void Update() {
		if (rigidbody.linearVelocity == Vector3.zero && rigidbody.angularVelocity == Vector3.zero) {
			//Destroy(gameObject, 30); //SPAWN PARTICLES
		}
	}

	public virtual void Initialize() {
		SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
		
		enabled = true;
		rigidbody.isKinematic = false;
		collider.enabled = true;
		pickupRange.enabled = true;
	}
	
	public virtual void Deactivate() {
		enabled = false;
		rigidbody.isKinematic = true;
		collider.enabled = false;
		pickupRange.enabled = false;
	}

	public Weapon PickUp(PlayerItemController player) {
		Deactivate();
		weapon.Initialize(player);
		return weapon;
	}
	
	//DO THIS IN A SINGLETON OR SOMETHING, IT EXISTS ALSO IN Weapon.cs
	public void SetLayerRecursively(GameObject obj, int newLayer) {
		// Update the layer of the object itself
		obj.layer = newLayer;

		// Update all children
		foreach (Transform child in obj.transform)
			SetLayerRecursively(child.gameObject, newLayer);
	}
	
}