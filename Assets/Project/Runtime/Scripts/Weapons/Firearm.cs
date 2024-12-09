using UnityEngine;

public enum ShootType {
	Automatic,
	Semiautomatic
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
		[SerializeField] private float aimZoomRatio;
		[SerializeField] private float muzzleVelocity;	//SHOULD THIS AND OTHER WEAPON-SPECIFIC PROJECTILE
		//ATTRIBUTES BE HERE OR IN THE PROJECTILE?
		
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
			Projectile newProjectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(shotDirection));
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