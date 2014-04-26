using UnityEngine;
using System.Collections;

public class Health : Powerup
{
	private const float HEALTH_AMOUNT = 50;
	private const float CHARACTER_MAX_HEALTH = 150;

	protected override void Activate(CharacterBase character)
	{
		character.Health += HEALTH_AMOUNT;

		if (character.Health > CHARACTER_MAX_HEALTH)
			character.Health = CHARACTER_MAX_HEALTH;
	}
}
