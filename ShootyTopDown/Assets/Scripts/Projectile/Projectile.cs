using UnityEngine;
using System.Collections;

[RequireComponent (typeof(LineRenderer))]
public class Projectile : MonoBehaviour
{
	private const float DESTRUCTION_TIME = 5.0f;

	private const float FADE_TIME = 0.4f;

	public GameObject shooter;
	public float damage = 10.0f;
	public Vector3 direction;

	public LayerMask layersToHit;

	private Vector3 startPos;
	private LineRenderer lineRenderer;

	private float creationTime;

	private float distance = 1000.0f;

	// Store initial position
	protected void Start()
	{
		startPos = transform.position;
		lineRenderer = GetComponent<LineRenderer>();
		creationTime = Time.time;

		// Raycast to get distance to hit
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position, direction, out hitInfo, 1000.0f, layersToHit))
		{
			distance = hitInfo.distance;

			CharacterBase character = hitInfo.collider.gameObject.GetComponent<CharacterBase>();

			// If this is a character
			if (character != null)
			{
				// Do damage
				character.Health -= damage;
				character.DamageTaken();
			}
		}
	}

	protected void Update()
	{
		// Update line renderer
		lineRenderer.SetPosition(0, transform.position);
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
