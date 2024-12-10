using System.Collections;
using FishNet.Object;
using UnityEngine;

public class Spawner : NetworkBehaviour {

	[SerializeField] private float spawnInterval = 20f;
	[SerializeField] private GameObject[] objectsToSpawn;

	private Coroutine spawnCoroutine;
	private float timer;

	protected void Start() {
		if (!IsServerInitialized) {
			enabled = false;
			return;
		}

		spawnCoroutine = StartCoroutine(Spawn());
	}

	private IEnumerator Spawn() {
		while (true) {
			timer = 0f;
			
			while (timer < spawnInterval) {
				timer += Time.deltaTime;
				yield return null;
			}
			
			SpawnObjectServer(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)]);
		}
	}

	private void SpawnObjectServer(GameObject objectToSpawn) {
		GameObject spawned = Instantiate(objectToSpawn, transform.position + Vector3.right + Vector3.down, Quaternion.identity);
		ServerManager.Spawn(spawned);
	}
}
