using UnityEngine;

public enum ShootType {
	Automatic,
	Semiautomatic
}

public class Firearm : Weapon {
	
	[Header("Internal References")]
	[SerializeField] private Transform muzzle;
	
	[Header("Shooting")]
	[SerializeField] private ShootType shootType;
	[SerializeField] private Projectile projectile;
	[SerializeField] private GameObject shell;
	[SerializeField] private int projectilesPerShot;
	[SerializeField] private float spreadAngle;

	[Header("Ammo")]
	[SerializeField] private int maxMags;
	[SerializeField] private int maxSpareMags;
	[SerializeField] private int startMags;
	[SerializeField] private int bulletsPerMag;
	
}