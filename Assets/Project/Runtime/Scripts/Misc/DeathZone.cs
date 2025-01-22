using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathZone : MonoBehaviour {

	[SerializeField] private bool disableRenderOnStart = true;
	private new BoxCollider collider;
	
	private void Awake() {
		collider = GetComponent<BoxCollider>();
		collider.isTrigger = true;
	}

	private void Start() {
		GetComponent<MeshRenderer>().enabled = !disableRenderOnStart;
	}

	private void OnTriggerEnter(Collider other) {
		
		PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
		if(playerHealth != null)
			playerHealth.TakeDamage(playerHealth.GetMaxHealth());
	}
}
