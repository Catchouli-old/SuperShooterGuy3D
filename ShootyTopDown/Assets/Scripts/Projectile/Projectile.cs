using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
	private const float DESTRUCTION_TIME = 5.0f;

	public GameObject shooter;
	public float damage = 10.0f;

	private Vector3 lastPos;

	// Store initial position
	protected void Start()
	{
		lastPos = transform.position;

		GetComponent<DestroyAfter>().time = DESTRUCTION_TIME;
	}
	
	// Handle collisions with characters
	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (enabled)
		{
			CharacterBase character = collision.collider.GetComponent<CharacterBase>();

			if (character != null)
			{
				character.Health -= damage;
				character.DamageTaken();

				if (character.Health <= 0.0f)
				{
					character.Dead = true;

					// Call character death handler and destroy the game object if it isn't overriden to return false
					if (character.Die())
						Destroy(character.gameObject);
				}

				GameObject.Destroy(gameObject);
			}
		}

		enabled = false;
	}
	
	public static Vector3 InterceptVector(Vector2 pos, Vector2 targetPos, Vector2 targetVel, float projectileSpeed)
	{
		// Find the difference between target and shooter pos
		Vector2 diff = targetPos - pos;
		
		// Find the direction to the target
		Vector2 dir = (targetPos - pos).normalized;
		
		// Project target velocity onto dir
		float vDotDir = Vector2.Dot(dir, targetVel);
		Vector2 uj = vDotDir * dir;
		
		// Subtract uj from u to get ui
		Vector2 ui = targetVel - uj;
		
		Vector2 vi = ui;
		
		float vmagsqr = targetVel.sqrMagnitude;
		float vimagsqr = vi.sqrMagnitude;
		
		float vjmag = Mathf.Sqrt(vmagsqr - vimagsqr);
		
		Vector2 vj = dir * vjmag;
		
		Vector2 v = vj + vi;
		
		return v;
	}
}
