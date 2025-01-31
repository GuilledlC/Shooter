﻿using FishNet.Object;
using UnityEngine;

public enum ShootType {
	Automatic,
	Semiautomatic,
	DoubleAction
}

//IMPLEMENT THIS
public enum FirearmState {
	Ready,
	Shooting,
	Reloading,
	Sheathing,
	Unsheathing
}

public class Firearm : Weapon {

	#region Attributes

		[Header("Internal Firearm References")]
		[SerializeField] private Transform muzzle;
		
		[Header("Shooting")]
		[SerializeField] private ShootType shootType;
		[SerializeField] private Projectile projectilePrefab;
		[SerializeField] private GameObject shellPrefab;
		[SerializeField] private int projectilesPerBullet;
		[SerializeField] private float spreadAngle;
		[Tooltip("Minimum duration between two shots")]
		[SerializeField] private float shotDelay;
		[Tooltip("Minimum duration between two shots when holding fire in a semiautomatic weapon")]
		[SerializeField] private float readyDelay; //Fanning? what should we do about this?
		[SerializeField] private float recoil; //DO SOMETHING ABOUT THIS
		[SerializeField] private float muzzleVelocity;	//SHOULD THIS AND OTHER WEAPON-SPECIFIC PROJECTILE
		//ATTRIBUTES BE HERE OR IN THE PROJECTILE?

		[Header("Aiming")]
		[Range(0f, 0.25f)]
		[SerializeField] private float aimDelay;
		[SerializeField] private float aimZoomRatio;
		private Transform playerAimPoint;
		
		[Header("Reload")]
		[SerializeField] private float reloadDelay;
		[SerializeField] private int loseBulletsOnReload;
		
		[Header("Ammo")]
		[SerializeField] private int maxMags;
		[SerializeField] private int maxSpareMags;
		[SerializeField] private int startMags;
		[SerializeField] private int bulletsPerMag; //+1 IN THE CHAMBER?
		public int currentAmmo { get; private set; } //DO SOMETHING ABOUT THIS
		public int currentMags { get; private set; } //DO SOMETHING ABOUT THIS
		private int chamber;
		
		[Header("Audiovisual")]
		[SerializeField] private Animator weaponAnimator;
		[SerializeField] private GameObject muzzleFlash;
		[SerializeField] private bool unParentMuzzleFlash; //SHOULD THIS BE TRUE ALWAYS?
		[SerializeField] private AudioClip shotSfx;
		[SerializeField] private AudioClip fullAutoSfx; //DO SOMETHING ABOUT THIS
		[SerializeField] private AudioClip gunMechanismsSfx;
		[SerializeField] private AudioClip switchWeaponSfx;
		[SerializeField] private AudioClip emptyChamberSfx;
		
		public GameObject Owner { get; private set; }
		public bool IsReloading { get; private set; }

	#endregion

	private void Awake() {
		currentAmmo = bulletsPerMag;
		currentMags = startMags;
		chamber = 1;
	}

	public override void Initialize(PlayerItemController player) {
		base.Initialize(player);
		playerAimPoint = player.GetAimPoint();
	}

	public void EasyShoot() {
		EasyShootServer();
	}
	
	[ServerRpc]
	private void EasyShootServer() {
		Projectile bullet = Instantiate(projectilePrefab, muzzle.position, transform.rotation);
		ServerManager.Spawn(bullet.gameObject);
		bullet.Initialize();
	}

	[ObserversRpc]
	private void EasyShootObserver() {
		
	}
	
	public void Aim() {
		MoveAimPoint(playerAimPoint.localPosition);
	}

	public void StopAiming() {
		MoveAimPoint(playerHoldPoint.localPosition);
	}

	private Vector3 weaponVelocityInternal = Vector3.zero;
	private void MoveAimPoint(Vector3 targetPoint) {
		transform.localPosition = targetPoint; /*Vector3.SmoothDamp(
			transform.localPosition,
			targetPoint,
			ref weaponVelocityInternal,
			aimDelay);*/
		//weaponPoint.localPosition = Vector3.Lerp(weaponPoint.localPosition, targetPosition, timeToAim);
	}
	
	public bool TryShoot() {
		if (BulletInChamber()) {
			if (true /* && delay between shots and all that jazz */) {
				HandleShoot();
				CycleAmmo();
				return true;
			}
		}
		else {
			//Empty click Sfx
		}
		return false;

	}

	private void HandleShoot() {
		chamber = 0;
		//Spawn all projectiles
		for (int i = 0; i < projectilesPerBullet; i++) {
			Vector3 shotDirection = GetShotDirectionWithinSpread();
			//Projectile newProjectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(shotDirection));
			//newProjectile.Shoot(this); //DO more weapon specific velocity and such settings
		}
		
		//Muzzle flash

		//Sound (both shoot and mechanisms)
		
		//Animation
		
		/*
		 OnShoot?.Invoke(); ?
		OnShootProcessed?.Invoke(); ?
		*/
	}

	private Vector3 GetShotDirectionWithinSpread() {
		return new Vector3(); //DO SOMETHING ABOUT THIS
	}
	
	public void Reload() {
		currentMags -= 1;
		currentAmmo = bulletsPerMag;
		//Play reload animation
		//When it is done:
		if (!BulletInChamber()) {
			//Play manual cycling animation
			CycleAmmo();
		}
	}

	private void CycleAmmo() {
		currentAmmo -= 1;
		chamber = 1;
	}
	
	public bool BulletInChamber() => chamber > 0;
	
	public void SwitchWeapon() {
		//Play switchWeaponSfx
	}
}