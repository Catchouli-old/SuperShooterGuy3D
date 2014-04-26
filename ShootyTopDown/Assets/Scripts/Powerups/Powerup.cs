using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour
{
	PowerupManager manager;

	protected void Start()
	{
		manager = (PowerupManager)GameObject.FindObjectOfType(typeof(PowerupManager));
	}

	protected virtual void Activate(CharacterBase character)
	{

	}

	protected void OnTriggerEnter(Collider collider)
	{
		CharacterBase character = collider.gameObject.GetComponent<CharacterBase>();

		if (character != null)
		{
			Activate(character);
			manager.PowerupCollected(gameObject);
		}
	}
}
