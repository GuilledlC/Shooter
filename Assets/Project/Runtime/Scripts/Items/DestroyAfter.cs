using System.Collections;
using UnityEngine;

public class DestroyAfter : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("How much time to destroy this object after it is dropped")]
	[SerializeField] private float destroyAfter = 90f;
	private Coroutine destructionCoroutine;
	private float timer;
	private bool isPaused = false;

	public float GetTimer() => timer;
	public bool GetPaused() => isPaused;
	
	public void StartDestruction() {
		if(destructionCoroutine != null)
			StopCoroutine(destructionCoroutine);

		timer = 0f;
		isPaused = false;
		destructionCoroutine = StartCoroutine(DestroyAfterDelayCoroutine());
	}

	public void StopDestruction() {
		isPaused = true;
		StopCoroutine(destructionCoroutine);
	}

	private IEnumerator DestroyAfterDelayCoroutine() {
		while (timer < destroyAfter) {
			if (!isPaused) {
				timer += Time.deltaTime;
			}
			yield return null;
		}

		Destroy(gameObject);
	}

}
