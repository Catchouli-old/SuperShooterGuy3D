using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PowerupManager : MonoBehaviour
{
	private const float RESPAWN_TIME = 5.0f;
	
	// I wanted to add a custom type to the inspector to make this
	// easy to modify but it turns out unity 4 broke it
	public GameObject healthPowerupPrefab;
	public int healthPowerupCount = 5;

	public GameObject automaticWeaponPrefab;
	public GameObject shotgunWeaponPrefab;
	public GameObject railgunWeaponPrefab;
	public int weaponCount = 1;

	public LayerMask actors;

	private CellGrid cellGrid;
	private List<GridCell> openSpaces;

	private List<Powerup> powerups;

	public void SetOpenSpaces(CellGrid cellGrid, List<GridCell> openSpaces)
	{
		this.cellGrid = cellGrid;
		this.openSpaces = new List<GridCell>(openSpaces);
		this.powerups = new List<Powerup>();
	}

	public void SpawnPowerups()
	{
		// See above
		SpawnPowerups(healthPowerupPrefab, healthPowerupCount);
		SpawnPowerups(automaticWeaponPrefab, weaponCount);
		SpawnPowerups(shotgunWeaponPrefab, weaponCount);
		SpawnPowerups(railgunWeaponPrefab, weaponCount);
	}
	
	public void DespawnPowerups()
	{
		if (powerups != null)
		{
			foreach (Powerup powerup in powerups)	
			{
				if (powerup != null && powerup.gameObject != null)
					Destroy(powerup.gameObject);
			}
			powerups.Clear();
		}
	}
	
	public void PowerupCollected(GameObject powerup)
	{
		powerups.Remove(powerup.GetComponent<Powerup>());

		// Deactivate it
		powerup.SetActive(false);
		
		// Respawn after RESPAWN_TIME secconds
		StartCoroutine(RespawnAfter(powerup, RESPAWN_TIME));
	}

	protected void SpawnPowerups(GameObject prefab, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			SpawnPowerup(prefab);
		}
	}

	protected void SpawnPowerup(GameObject prefab)
	{
		const int MAX_ITERATIONS = 5;

		// Try to spawn a prefab at a random position MAX_ITERATIONS times
		// then give up and mark the powerup as collected
		bool spawned = false;
		for (int i = 0; i < MAX_ITERATIONS; ++i)
		{
			GridCell cell = openSpaces[UnityEngine.Random.Range(0, openSpaces.Count)];
			Vector3 worldPos = cellGrid.GetCellPos(cell);
			worldPos.z = 0;

			if (!Physics.CheckSphere(worldPos, 0.1f, actors))
			{
				// Spawn powerup
				GameObject powerup = (GameObject)GameObject.Instantiate(prefab, worldPos, Quaternion.identity);

				// Get rid of the "cloned" text in its name
				powerup.name = powerup.name.Substring(0, powerup.name.Length - 7);

				spawned = true;
				powerup.SetActive(true);
				powerups.Add(powerup.GetComponent<Powerup>());
				break;
			}
		}

		// Give up and mark as collected so it'll respawn soon
		if (!spawned)
		{
			GameObject powerup = (GameObject)GameObject.Instantiate(prefab);
			powerups.Add(powerup.GetComponent<Powerup>());
			PowerupCollected(powerup);
		}
	}

	IEnumerator RespawnAfter(GameObject powerup, float time)
	{
		yield return new WaitForSeconds(time);

		if (powerup != null)
		{
			// Spawn a new powerup and destroy the old one
			SpawnPowerup(powerup);
			Destroy(powerup);
		}
	}
}
