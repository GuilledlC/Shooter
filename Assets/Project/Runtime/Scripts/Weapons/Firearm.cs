using UnityEngine;

public enum ShootType {
	Automatic,
	Semiautomatic
}

public class Firearm : Weapon {
	
	[Header("Internal firearm References")]
	[SerializeField] private Transform muzzle;
	
	[Header("Shooting")]
	[SerializeField] private ShootType shootType;
	[SerializeField] private Projectile projectilePrefab;
	[SerializeField] private GameObject shellPrefab;
	[SerializeField] private int projectilesPerShot;
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
	[SerializeField] private int bulletsPerShot; //Rare case but imagine if you can shoot two bullets in one frame?
	private int currentAmmo; //DO SOMETHING ABOUT THIS
	private int currentMags; //DO SOMETHING ABOUT THIS
	
	[Header("Audiovisual")]
	[SerializeField] private Animator weaponAnimator;
	[SerializeField] private GameObject muzzleFlash;
	[SerializeField] private bool unParentMuzzleFlash;
	[SerializeField] private AudioClip shotSfx;
	[SerializeField] private AudioClip fullAutoSfx; //DO SOMETHING ABOUT THIS
	[SerializeField] private AudioClip gunMechanismsSfx;
	[SerializeField] private AudioClip switchWeaponSfx;
	[SerializeField] private AudioClip emptyChamberSfx;
	
	public GameObject Owner { get; set; }
	public bool IsReloading { get; set; }

	private void Awake() {
		currentAmmo = bulletsPerMag;
		currentMags = startMags;
	}

	public bool TryShoot() {
		if (currentAmmo > bulletsPerShot) {
			if (true /* && delay between shots and all that jazz */) {
				for (int i = 0; i < bulletsPerShot; i++) {
					HandleShoot();
					currentAmmo -= 1;
				}
				return true;
			}
		}
		else {
			//Empty click Sfx
		}
		return false;

	}

	private void HandleShoot() {
		//Spawn all projectiles
		for (int i = 0; i < projectilesPerShot; i++) {
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
		
	}

	public void SwitchWeapon() {
		//Play switchWeaponSfx
	}
}