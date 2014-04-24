using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof(Maze))]
public class LevelGen : MonoBehaviour
{
	private int MAX_LIGHT_COUNT = 100;

	public static LevelGen Instance { get { return instance; } }

	public static CellGrid CellGrid { get { return new CellGrid(Grid, instance.transform); } }
	public static GridCell[,] Grid { get; set; }

	private const float LIGHT_GEN_CHANCE = 0.5f;

	public GameObject wallLightPrefab;

	public GameObject enemyPrefab;

	public int enemyCount = 10;
	public int lightCount = 100;

	private static LevelGen instance;

	private Maze maze;
	private GameObject lights;
	private GameObject oldLights;

	private Mesh mesh;

	private GameObject enemies;

	private List<Light> lightsPool;

	public LevelGen()
	{
		instance = this;
	}

	public void Ungenerate()
	{
		// Clean up old generation
		DestroyIfNotNull(mesh);
		DestroyIfNotNull(enemies);
	}

	public void Regenerate()
	{
		// Clean up
		Ungenerate();

		// Regenerate maze
		maze.Regenerate();
		
		// Create cell grid for pathfinding
		CellGrid cellGrid = new CellGrid(Grid, transform);
		
		// Copy maze to pathfinding grid
		Grid = new GridCell[maze.width, maze.height];
		for (int x = 0; x < maze.width; ++x)
		{
			for (int y = 0; y < maze.height; ++y)
			{
				Grid[x, y] = new GridCell(x, y, maze.Grid[x, y].visited);
			}
		}

		// Reset enemy pathfinding data
		NonplayerCharacter.ResetPathfinding();
		
		// Get list of dead ends
		List<Cell> deadEnds = new List<Cell>(maze.DeadEnds);
		
		// Pick random deadend for player
		Cell playerSpawn = maze.DeadEnds[Random.Range(0, maze.DeadEnds.Count)];
		
		// Remove this dead end from the list so that no enemies can spawn there
		deadEnds.Remove(playerSpawn);
		
		// Spawn the player there
		Vector3 playerPos = cellGrid.GetCellPos(new GridCell(playerSpawn.position.x, playerSpawn.position.y));
		playerPos.z = 0;
		
		GameObject playerObj = ((Component)GameObject.FindObjectOfType(typeof(PlayerCharacter))).gameObject;
		playerObj.transform.position = playerPos;

		// Create enemies and do the same
		enemies = new GameObject("Enemies");
		for (int i = 0; i < enemyCount; ++i)
		{
			Cell deadEnd = deadEnds[Random.Range(0, deadEnds.Count)];
			
			Vector3 enemyPos = cellGrid.GetCellPos(new GridCell(deadEnd.position.x, deadEnd.position.y));
			enemyPos.z = 0;
			
			GameObject enemy = (GameObject)GameObject.Instantiate(enemyPrefab,
			                                                      enemyPos,
			                                                      Quaternion.identity);
			enemy.GetComponent<NonplayerCharacter>().target = playerObj;

			enemy.transform.parent = enemies.transform;

			// Add a slight force to enemy so they get unstuck if they're overlapping another character
			enemy.rigidbody.AddForce(new Vector3(0.1f, 0.1f));
		}
		
		// Spawn powerups in maze
		PowerupManager powerupManager = (PowerupManager)GameObject.FindObjectOfType(typeof(PowerupManager));
		powerupManager.SetDeadEnds(cellGrid, deadEnds);
		powerupManager.SpawnPowerups();
		
		// Create mesh for level
		mesh = MeshGenerator.GenerateMesh(maze.Grid);
		
		// Asign mesh to mesh filter
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;

		// Create lights on walls
		if (wallLightPrefab != null)
		{
			CreateLights();
		}
	}

	void CreateLights()
	{
		// Shuffle walls
		MeshGenerator.Walls.Shuffle();

		int i = 0;
		foreach (MeshGenerator.WallDef wall in MeshGenerator.Walls)
		{
			if (i >= lightsPool.Count)
				break;

			// Dont generate lights for outer walls
			if (wall.outerWall == true)
				continue;
			
			if (Random.Range(0.0f, 1.0f) < LIGHT_GEN_CHANCE)
				continue;
			
			Vector3 worldPos = wall.pos;
			
			// Scale, rotate and translate the positions
			worldPos.Scale(transform.lossyScale);
			worldPos = transform.rotation * worldPos;
			worldPos += transform.position;
			
			// Put them at the top of the walls
			worldPos.z += transform.localScale.y * -0.5f;
			
			// Offset the position by the wall normal
			const float offset = 0.1f;
			worldPos += wall.normal * offset;

			lightsPool[i].transform.position = worldPos;
			lightsPool[i].transform.rotation = transform.rotation;
			
			lightsPool[i].transform.LookAt(worldPos + wall.normal, Vector3.back);

			lightsPool[i].enabled = true;

			++i;
		}

		// Disable the rest of the lights
		for (; i < lightsPool.Count; ++i)
		{
			lightsPool[i].enabled = false;
		}
	}

	// Generate maze
	private void Start()
	{
		maze = GetComponent<Maze>();

		// Dynamic lights don't work great so we use a pool of lights
		GameObject lights = new GameObject("Lights");
		lightsPool = new List<Light>();
		lightCount = MAX_LIGHT_COUNT;
		for (int i = 0; i < lightCount; ++i)
		{
			GameObject light = (GameObject)GameObject.Instantiate(wallLightPrefab);
			light.transform.parent = lights.transform;

			lightsPool.Add(light.GetComponent<Light>());
		}
	}
	
	private void DestroyIfNotNull(UnityEngine.Object obj)
	{
		if (obj != null)
		{
			Destroy(obj);
		}
	}
}
