using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour
{
	protected virtual void Activate(CharacterBase character)
	{

	}

	protected void OnTriggerEnter(Collider collider)
	{
		CharacterBase character = collider.gameObject.GetComponent<CharacterBase>();

		if (character != null)
		{
			Activate(character);
			Destroy(gameObject);
		}
	}
}
