using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

	[Header("Basic attributes")]
	[SerializeField] private float maxHealth;
	[SerializeField] private float startHealth;

	private float currentHealth;

	public void Initialize() {
		//Initialize the player's health
		currentHealth = startHealth;
		OnHealthChanged?.Invoke(currentHealth, maxHealth);
	}

	public void Heal(float amount) {
		
		//Validate the healing amount and update the current health
		currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
		
		//Notify other systems (e.g., UI, networking) of the health change
		OnHealthChanged?.Invoke(currentHealth, maxHealth);
	}

	public void TakeDamage(float amount) {
		
		//Validate the damage amount and update the current health
		currentHealth = Mathf.Max(currentHealth - amount, 0f);
		
		// Notify other systems (e.g., UI, networking) of the health change
		OnHealthChanged?.Invoke(currentHealth, maxHealth);
		if(currentHealth <= 0f)
			OnPlayerDeath?.Invoke();
	}

	public float GetMaxHealth() => maxHealth;
	public float GetCurrentHealth() => currentHealth;

	public event Action<float, float> OnHealthChanged;
	public event Action OnPlayerDeath;

}
