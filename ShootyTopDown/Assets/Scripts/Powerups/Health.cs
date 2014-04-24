using UnityEngine;
using System.Collections;

public class Health : Powerup
{
	private const float HEALTH_AMOUNT = 50;

	protected override void Activate(CharacterBase character)
	{
		character.Health += HEALTH_AMOUNT;
	}
}
