using UnityEngine;
using System.Collections;

public class CharacterBase
	: MonoBehaviour
{
	public enum TurningMode
	{
		INSTANT,
		SMOOTH
	};

	public enum WeaponType
	{
		PISTOL,
		SHOTGUN,
		AUTOMATIC,
		RAILGUN
	};

	public bool Dead { get; set; }

	public static float MaxHealth { get { return 100; } }
	public float Health { get; set; }
	
	public WeaponType CurrentWeapon { get { return currentWeapon; } set { currentWeapon = value; } }
	public int CurrentAmmo { get { return currentAmmo; } set { currentAmmo = value; } }

	public Vector2 FacingDirection { get { return facingDirectionReal; } }

	public float Cooldown { get { return cooldown; } }

	public const float MOVEMENT_FORCE = 70.0f;
	public const float MOVEMENT_TERMINAL_VELOCITY = 10.0f;

	public const float EXTRA_HEALTH_DRAIN_SPEED = 2.5f;
	public const float HEALTH_REGEN_PER_SECOND = 1.0f;

	public const float TURN_SPEED = 15.0f;
	public const TurningMode TURN_MODE = TurningMode.SMOOTH;

	public const float BULLET_SPEED = 20.0f;

	public const float BULLET_DELAY = 0.1f;
	
	private const float PISTOL_DELAY = 0.35f;
	private const float PISTOL_DAMAGE = 10.0f;
	private const float PISTOL_JITTER = 1.0f;

	private const float AUTOMATIC_DELAY = 0.1f;
	private const float AUTOMATIC_DAMAGE = 10.0f;
	private const float AUTOMATIC_JITTER = 5.0f;
	
	private const float SHOTGUN_DELAY = 1.0f;
	private const float SHOTGUN_DAMAGE = 10.0f;
	private const float SHOTGUN_JITTER = 9.0f;
	
	private const int SHOTGUN_FRAGMENTS_MIN = 5;
	private const int SHOTGUN_FRAGMENTS_MAX = 10;
	
	private const float RAILGUN_DELAY = 1.5f;
	private const float RAILGUN_DAMAGE = 100.0f;
	private const float RAILGUN_JITTER = 0.1f;

	// The prefab for projectiles
	public GameObject projectilePrefab;

	// The current weapon we have
	protected WeaponType currentWeapon = WeaponType.PISTOL;
	protected int currentAmmo = 0;

	// The current direction we're facing in
	protected Vector2 facingDirectionTarget = Vector2.right;

	// The real direction we're facing in
	private Vector2 facingDirectionReal = Vector2.right;

	// Timer for bullets
	private float lastBulletTime= -10.0f;
	private float cooldown = 1.0f;

	// Turning mode
	private TurningMode turningMode = TURN_MODE;
	
	// Damage taken event handler
	public virtual void DamageTaken()
	{

	}
	
	// Death event handler
	// Return true to destroy object
	public virtual bool Die()
	{
		return true;
	}

	public void SwitchWeapon(WeaponType newWeapon, int ammo)
	{
		lastBulletTime = 0;
		cooldown = 1;
		CurrentWeapon = newWeapon;
		CurrentAmmo = ammo;
	}

	protected virtual void Start()
	{
		Health = MaxHealth;
		lastBulletTime = Time.time;
	}

	protected void Move(Vector2 input)
	{
		// Make sure input magnitude clamped -1..1 in each direction
		if (input.sqrMagnitude > 1.0f)
			input = input.normalized;

		// Add force
		rigidbody.AddForce(MOVEMENT_FORCE * input * Time.timeScale * Time.timeScale);

		// Clamp speed
		float speed = rigidbody.velocity.magnitude;

		if (speed > MOVEMENT_TERMINAL_VELOCITY)
			rigidbody.velocity =
				rigidbody.velocity.normalized * MOVEMENT_TERMINAL_VELOCITY;
	}
	
	protected void TurnTowards(Vector2 direction)
	{
		facingDirectionTarget = direction;
	}

	protected void Fire()
	{
		Debug.Log(WeaponTypeToName(CurrentWeapon) + " ammo: " + CurrentAmmo.ToString());

		switch (CurrentWeapon)
		{
		case WeaponType.PISTOL:
			FirePistol();
			break;
		case WeaponType.AUTOMATIC:
			FireAutomatic();
			break;
		case WeaponType.SHOTGUN:
			FireShotgun();
			break;
		case WeaponType.RAILGUN:
			FireRailgun();
			break;
		}
	}

	protected void FirePistol()
	{
		if (cooldown >= 1)
		{
			CurrentAmmo--;
			
			// Update bullet timer
			lastBulletTime = Time.time;

			// Fire bullet
			FireBullet(facingDirectionReal, PISTOL_DAMAGE, PISTOL_JITTER);
		}
	}

	protected void FireAutomatic()
	{
		if (cooldown >= 1)
		{
			CurrentAmmo--;

			// Update bullet timer
			lastBulletTime = Time.time;
			
			// Fire bullet
			FireBullet(facingDirectionReal, AUTOMATIC_DAMAGE, AUTOMATIC_JITTER);
		}
	}

	protected void FireShotgun()
	{
		if (cooldown >= 1)
		{
			CurrentAmmo--;

			// Randomise fragment count
			int fragments = Random.Range(SHOTGUN_FRAGMENTS_MIN, SHOTGUN_FRAGMENTS_MAX+1);
			
			// Update bullet timer
			lastBulletTime = Time.time;
			
			// Fire bullets
			for (int i = 0; i < fragments; ++i)
				FireBullet(facingDirectionReal, SHOTGUN_DAMAGE, SHOTGUN_JITTER);
		}
	}

	protected void FireRailgun()
	{
		if (cooldown >= 1)
		{
			CurrentAmmo--;
			
			// Update bullet timer
			lastBulletTime = Time.time;
			
			// Fire bullet
			FireBullet(facingDirectionReal, RAILGUN_DAMAGE, RAILGUN_JITTER);
		}
	}

	protected void FireBullet(Vector3 direction, float damage, float jitter)
	{
		// Create projectile
		GameObject projectile = (GameObject)GameObject.Instantiate(projectilePrefab);
		
		// Set shooter and damage
		projectile.GetComponent<Projectile>().shooter = gameObject;
		projectile.GetComponent<Projectile>().damage = damage;
		projectile.GetComponent<Projectile>().direction =
			Quaternion.AngleAxis(Random.Range(-jitter, jitter), Vector3.forward) * direction;
		
		// Place projectile on edge of character
		projectile.transform.position = transform.position + (Vector3)facingDirectionTarget * transform.localScale.x * 0.5f;
	}

	// Rotate to facing direction
	protected virtual void Update()
	{
		if (CurrentAmmo <= 0 && CurrentWeapon != WeaponType.PISTOL)
		{
			CurrentWeapon = WeaponType.PISTOL;
		}

		if (Health <= 0.0f)
		{
			Dead = true;
			
			// Call character death handler and destroy the game object if it isn't overriden to return false
			if (Die())
				Destroy(gameObject);

			enabled = false;
			return;
		}

		switch (turningMode)
		{
		case TurningMode.INSTANT:
			facingDirectionReal = facingDirectionTarget;
			break;
		case TurningMode.SMOOTH:
			facingDirectionReal = Vector3.Lerp(facingDirectionReal, facingDirectionTarget, TURN_SPEED * Time.deltaTime);
			break;
		}
		
		switch (CurrentWeapon)
		{
		case WeaponType.PISTOL:
			cooldown = Mathf.Clamp((Time.time - lastBulletTime) / PISTOL_DELAY, 0.0f, 1.0f);
			break;
		case WeaponType.AUTOMATIC:
			cooldown = Mathf.Clamp((Time.time - lastBulletTime) / AUTOMATIC_DELAY, 0.0f, 1.0f);
			break;
		case WeaponType.SHOTGUN:
			cooldown = Mathf.Clamp((Time.time - lastBulletTime) / SHOTGUN_DELAY, 0.0f, 1.0f);
			break;
		case WeaponType.RAILGUN:
			cooldown = Mathf.Clamp((Time.time - lastBulletTime) / RAILGUN_DELAY, 0.0f, 1.0f);
			break;
		}

		if (Health > 100)
		{
			Health -= Time.deltaTime * EXTRA_HEALTH_DRAIN_SPEED;

			if (Health < 100)
				Health = 100;
		}

		if (Health < 100)
		{
			//Health += HEALTH_REGEN_PER_SECOND * Time.deltaTime;

			if (Health > 100)
				Health = 100;
		}

		// Update facing direction
		float zRot = Mathf.Atan2(facingDirectionReal.y, facingDirectionReal.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, zRot - 90);
	}

	public static string WeaponTypeToName(WeaponType weaponType)
	{
		switch (weaponType)
		{
		case WeaponType.PISTOL:
			return "Pistol";
		case WeaponType.SHOTGUN:
			return "Shotgun";
		case WeaponType.AUTOMATIC:
			return "Automatic";
		case WeaponType.RAILGUN:
			return "Railgun";
		default:
			return "Unnamed";
		}
	}
}
