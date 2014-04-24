using UnityEngine;
using System.Collections;

public class PlayerCharacter
	: CharacterBase
{
	// Don't destroy on death
	public override bool Die()
	{
		return false;
	}

	// Handle input
	protected override void Update()
	{
		// Call base update method
		base.Update();

		// Handle input
		// Get movement axes
		float horz = Input.GetAxis("Horizontal");
		float vert = Input.GetAxis("Vertical");

		// Get fire button
		bool fire = Input.GetButton("Fire1");

		// Get direction to mouse cursor
		Vector3 playerPos = transform.position;

		// Calcualte player pos in screen space
		playerPos.z = Camera.main.nearClipPlane;
		Vector3 playerPosScreen = Camera.main.WorldToScreenPoint(playerPos);
		
		// Move player pos screen to depth 0
		playerPosScreen.z = 0;

		// Calculate direction
		Vector3 mouseDir = (Input.mousePosition - playerPosScreen).normalized;

		// Move
		Move(new Vector2(horz, vert));
		TurnTowards(mouseDir);

		// Fire
		if (fire)
			Fire();
	}
}
