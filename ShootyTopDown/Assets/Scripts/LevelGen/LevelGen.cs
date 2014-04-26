using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof(Maze))]
public class LevelGen : MonoBehaviour
{
	public const int MAZE_RESOLUTION = 2;

	private const int MAX_LIGHT_COUNT = 100;

	public static LevelGen Instance { get { return instance; } }

	public static CellGrid CellGrid { get { return new CellGrid(Grid, instance.transform, MAZE_RESOLUTION); } }
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

	private List<GridCell> emptySpaces;

	private Mesh mesh;

	private GameObject enemies;

	private List<Light> lightsPool;

	private Cell[,] hiResGrid;
	private CellGrid cellGrid;

	public LevelGen()
	{
		instance = this;
	}

	public void Ungenerate()
	{
		// Clean up old generation
		DestroyIfNotNull(mesh);
		DestroyIfNotNull(enemies);

		((PowerupManager)GameObject.FindObjectOfType(typeof(PowerupManager))).DespawnPowerups();
	}

	public void Regenerate()
	{
		// Clean up
		Ungenerate();

		// Regenerate maze
		maze.Regenerate();
		
		// Make the mesh seriously like 5x the resolution of the original maze for destructivity
		hiResGrid = new Cell[maze.Grid.GetLength(0) * MAZE_RESOLUTION,
		                             maze.Grid.GetLength(1) * MAZE_RESOLUTION];
		
		for (int x = 0; x < maze.Grid.GetLength(0); ++x)
		{
			for (int y = 0; y < maze.Grid.GetLength(1); ++y)
			{
				for (int ix = 0; ix < MAZE_RESOLUTION; ++ix)
				{
					for (int iy = 0; iy < MAZE_RESOLUTION; ++iy)
					{
						Cell oldCell = maze.Grid[x, y];
						
						int newX = x * MAZE_RESOLUTION + ix;
						int newY = y * MAZE_RESOLUTION + iy;
						
						Cell newCell = new Cell(oldCell);
						newCell.position = new Point(newX, newY);
						
						hiResGrid[newX, newY] = newCell;
					}
				}
			}
		}
		
		// Store a list of empty spaces
		emptySpaces = new List<GridCell>();
		
		// Copy maze to pathfinding grid
		int width = hiResGrid.GetLength(0);
		int height = hiResGrid.GetLength(1);
		Grid = new GridCell[width, height];
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				Grid[x, y] = new GridCell(x, y, hiResGrid[x, y].visited);

				if (Grid[x, y].Accessible)
					emptySpaces.Add(Grid[x, y]);
			}
		}
		
		// Create cell grid for pathfinding
		cellGrid = CellGrid;

		// Reset enemy pathfinding data
		NonplayerCharacter.ResetPathfinding();
		
		// Get list of empty cells
		List<GridCell> emptyCellsCopy = new List<GridCell>(emptySpaces);

		// Pick random deadend for player
		GridCell playerSpawnCell = emptyCellsCopy[Random.Range(0, emptyCellsCopy.Count)];
		
		// Remove this dead end from the list so that no enemies can spawn there
		emptyCellsCopy.Remove(playerSpawnCell);
		
		// Spawn the player there
		Vector3 playerPos = cellGrid.GetCellPos(playerSpawnCell);
		playerPos.z = 0;
		
		GameObject playerObj = ((Component)GameObject.FindObjectOfType(typeof(PlayerCharacter))).gameObject;
		playerObj.transform.position = playerPos;

		// Create enemies and do the same
		enemies = new GameObject("Enemies");
		for (int i = 0; i < enemyCount; ++i)
		{
			// Pick random place for enemy to spawn
			GridCell enemySpawnCell = emptyCellsCopy[Random.Range(0, emptyCellsCopy.Count)];

			Vector3 enemyPos = cellGrid.GetCellPos(enemySpawnCell);
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
		powerupManager.SetOpenSpaces(cellGrid, emptySpaces);
		powerupManager.SpawnPowerups();

		// Create mesh for level
		mesh = MeshGenerator.GenerateMesh(hiResGrid);
		
		// Asign mesh to mesh filter
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;

		// Create lights on walls
		if (wallLightPrefab != null)
		{
			CreateLights();
		}
	}

	public void BreakWall(Vector3 point)
	{
		Point cell = cellGrid.GetIdxFromPos(point);
		
		int width = hiResGrid.GetLength(0);
		int height = hiResGrid.GetLength(1);

		if (cell.x >= 0 && cell.y >= 0 &&
		    cell.x < width && cell.y < height)
		{
			hiResGrid[cell.x, cell.y].visited = true;

			emptySpaces.Add(cellGrid.Grid[cell.x, cell.y]);

			// Regenerate mesh
			Destroy(mesh);
			mesh = MeshGenerator.GenerateMesh(hiResGrid);
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
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
