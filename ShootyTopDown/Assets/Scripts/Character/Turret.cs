using UnityEngine;
using System.Collections;

public class Turret
	: CharacterBase
{
	public enum Dumbness
	{
		VERY,
		NOTVERY
	}

	public Transform target;

	public Dumbness dumbness = Dumbness.VERY;

	protected override void Update()
	{
		base.Update();

		Vector3 targetDir = Vector3.zero;

		switch (dumbness)
		{
		case Dumbness.VERY:

			targetDir = (target.transform.position - transform.position).normalized;

			break;
		case Dumbness.NOTVERY:
			
			targetDir = Projectile.InterceptVector(transform.position, target.transform.position, target.rigidbody.velocity, BULLET_SPEED).normalized;

			break;
		}
		
		TurnTowards(targetDir);
		Fire();
	}
}
