using UnityEngine;
using System.Collections;

[RequireComponent (typeof(LineRenderer))]
public class Projectile : MonoBehaviour
{
	public static float DamageMultiplier { get { return damageMultiplier; } set { damageMultiplier = value; } }

	private const float DESTRUCTION_TIME = 5.0f;

	private const float FADE_TIME = 0.4f;

	// An empty layer - used temporarily for raycasts so that they avoid the current character
	public LayerMask emptyLayer;

	public GameObject shooter;
	public float damage = 10.0f;
	public Vector3 direction;

	public LayerMask layersToHit;

	private LineRenderer lineRenderer;

	private float creationTime;

	private float distance = 1000.0f;

	private static float damageMultiplier = 1.0f;

	// Store initial position
	protected void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		creationTime = Time.time;

		// Store shooter's old layermask
		OldLayer oldLayer = shooter.AddComponent<OldLayer>();
		oldLayer.oldlayer = shooter.layer;
		oldLayer.ChangeObjectLayermask(emptyLayer);

		// Raycast to check hit
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position, direction, out hitInfo, 1000.0f, layersToHit))
		{
			distance = hitInfo.distance;

			CharacterBase character = hitInfo.collider.gameObject.GetComponent<CharacterBase>();

			// If this is a character
			if (character != null)
			{
				// Do damage
				character.Health -= damage * damageMultiplier;
				if (character.Health < 0)
					character.Health = 0;
				character.DamageTaken();
			}
			// If this is a wall
			else
			{
				Vector3 hitPoint = hitInfo.point;

				// Sink hitpoint into wall a tiny bit
				hitPoint -= hitInfo.normal * 0.001f;

				((LevelGen)GameObject.FindObjectOfType(typeof(LevelGen))).BreakWall(hitPoint);
			}
		}

		// Restore shooter's layermask
		oldLayer.RestoreObjectLayer();
		Destroy(oldLayer);
	}

	protected void Update()
	{
		lineRenderer.SetPosition(0, transform.position + direction);
		lineRenderer.SetPosition(1, transform.position + distance * direction);

		// Calculate time since spawn
		float dt = Time.time - creationTime;
		float coef = dt / FADE_TIME;

		// Fade based on time since spawn
		Color col = lineRenderer.material.color;
		col.a = Mathf.Lerp(1.0f, 0.0f, coef);
		lineRenderer.material.color = col;

		// Destroy if this is done fading out
		if (coef >= 1.0f)
			Destroy(gameObject);
	}
}
