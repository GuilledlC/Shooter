using System;
using UnityEngine;
using FishNet.Component.Transforming;
using FishNet.Object;

[RequireComponent(typeof(DestroyAfter))]
public class Projectile : MonoBehaviour {

	[SerializeField] private float speed;
	
	private void Update() {
		Move(Time.deltaTime);
	}

	public void Initialize() {
		enabled = true;
	}
	
	private void Move(float deltaTime) {
		transform.position += transform.forward * (speed * deltaTime);
	}
}