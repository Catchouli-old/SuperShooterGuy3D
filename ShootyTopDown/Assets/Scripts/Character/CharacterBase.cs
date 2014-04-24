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

	public const float MOVEMENT_FORCE = 70.0f;
	public const float MOVEMENT_TERMINAL_VELOCITY = 10.0f;

	public const float EXTRA_HEALTH_DRAIN_SPEED = 2.5f;
	public const float HEALTH_REGEN_PER_SECOND = 1.0f;

	public const float TURN_SPEED = 15.0f;
	public const TurningMode TURN_MODE = TurningMode.SMOOTH;

	public const float BULLET_SPEED = 20.0f;

	public const float BULLET_DELAY = 0.1f;

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
	private float lastBulletTime;

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
		if (CurrentAmmo <= 0 && CurrentWeapon != WeaponType.PISTOL)
		{
			CurrentWeapon = WeaponType.PISTOL;
		}
		else
		{
			CurrentAmmo--;
		}

		if (Time.time - lastBulletTime >= BULLET_DELAY)
		{
			// Update bullet timer
			lastBulletTime = Time.time;

			// Create projectile
			GameObject projectile = (GameObject)GameObject.Instantiate(projectilePrefab);

			// Set shooter and damage
			projectile.GetComponent<Projectile>().shooter = gameObject;
			projectile.GetComponent<Projectile>().damage = 10.0f;

			// Place projectile on edge of character
			projectile.transform.position = transform.position + (Vector3)facingDirectionTarget * transform.localScale.x * 0.5f;

			// Set projectile velocity to direction
			projectile.rigidbody.velocity = facingDirectionReal * BULLET_SPEED;

			// Disable collisions between characters and their own bullets
			Physics.IgnoreCollision(collider, projectile.collider);
		}
	}

	// Rotate to facing direction
	protected virtual void Update()
	{
		switch (turningMode)
		{
		case TurningMode.INSTANT:
			facingDirectionReal = facingDirectionTarget;
			break;
		case TurningMode.SMOOTH:
			facingDirectionReal = Vector3.Lerp(facingDirectionReal, facingDirectionTarget, TURN_SPEED * Time.deltaTime);
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
