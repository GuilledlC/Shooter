using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathZone : MonoBehaviour {

	private BoxCollider collider;
	
	private void Awake() {
		collider = GetComponent<BoxCollider>();
		collider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other) {
		
		PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
		if(playerHealth != null)
			playerHealth.TakeDamage(playerHealth.GetMaxHealth());
	}
}
