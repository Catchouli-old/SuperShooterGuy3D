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

	public LayerMask actors;

	private CellGrid cellGrid;
	private List<Cell> deadEnds;

	public void SetDeadEnds(CellGrid cellGrid, List<Cell> deadEnds)
	{
		this.cellGrid = cellGrid;
		this.deadEnds = new List<Cell>(deadEnds);
	}

	public void SpawnPowerups()
	{
		// See above
		SpawnPowerups(healthPowerupPrefab, healthPowerupCount);
	}
	
	public void PowerupCollected(GameObject powerup)
	{
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
			Cell cell = deadEnds[UnityEngine.Random.Range(0, deadEnds.Count)];
			Vector3 worldPos = cellGrid.GetCellPos(new GridCell(cell.position.x, cell.position.y));
			worldPos.z = 0;

			if (!Physics.CheckSphere(worldPos, 0.1f, actors))
			{
				spawned = true;
				GameObject.Instantiate(prefab, worldPos, Quaternion.identity);
				break;
			}
		}

		// Give up and mark as collected so it'll respawn soon
		if (!spawned)
		{
			GameObject powerup = (GameObject)GameObject.Instantiate(prefab);
			PowerupCollected(powerup);
		}
	}

	IEnumerator RespawnAfter(GameObject powerup, float time)
	{
		yield return new WaitForSeconds(time);

		// Spawn a new powerup and destroy the old one
		SpawnPowerup(powerup);
		Destroy(powerup);
	}
}
