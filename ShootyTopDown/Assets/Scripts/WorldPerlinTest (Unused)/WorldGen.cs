using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public class WorldGen : MonoBehaviour
{
	private const int MAX_TILES = 10;

	private struct Tile
	{
		public readonly int x, y;
		
		public Tile(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public enum SeedMode
	{
		FIXED,
		TIME,
		RANDOM
	}
	
	public Transform target;

	public GameObject worldTilePrefab;
	
	public SeedMode seedMode = SeedMode.FIXED;
	public float seed = 0.0f;
	
	private float tileWidth { get { return worldTilePrefab.transform.lossyScale.x; } }
	private float tileHeight { get { return worldTilePrefab.transform.lossyScale.y; } }

	private IDbConnection conn;

	private Dictionary<Tile, GameObject> tiles;

	protected void Start()
	{
		// Set seed
		switch (seedMode)
		{
		case SeedMode.RANDOM:
			seed = Random.Range(-1000.0f, 1000.0f);
			break;
		case SeedMode.TIME:
			seed = Time.time;
			break;
		default:
			break;
		}

		tiles = new Dictionary<Tile, GameObject>();
	}

	protected void Update()
	{
		Vector3 targetPos;

		if (target == null)
		{
			// Get mouse position in world space
			targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else
		{
			targetPos = target.position;
		}

		// Get tile x and y idx
		int xc = (int)Mathf.Floor(targetPos.x / tileWidth);
		int yc = (int)Mathf.Floor(targetPos.y / tileHeight);
		
		CreateTile(xc+0, yc-1);
		CreateTile(xc-1, yc-1);
		CreateTile(xc+1, yc-1);
		CreateTile(xc+0, yc-0);
		CreateTile(xc-1, yc-0);
		CreateTile(xc+1, yc-0);
		CreateTile(xc+0, yc+1);
		CreateTile(xc-1, yc+1);
		CreateTile(xc+1, yc+1);

		// Remove farthest away tile if count > MAX_TILES
		// TODO: causes problems - probably because the hash isn't unique for the map?
		// it'd be better to use a database
		/*if (tiles.Count > MAX_TILES)
		{
			Vector3 newTilePos = new Vector3(x, y);

			KeyValuePair<Tile, GameObject> farthest;
			float farthestDistance;

			// Get first item in map
			farthest = tiles.First();
			farthestDistance =
				(newTilePos - farthest.Value.transform.position).sqrMagnitude;

			foreach (KeyValuePair<Tile, GameObject> tile in tiles)
			{
				GameObject tileObj = tile.Value;

				Vector3 tilePos = tileObj.transform.position;

				float dist = (tilePos - newTilePos).sqrMagnitude;

				if (dist > farthestDistance)
				{
					farthest = tile;
					farthestDistance = dist;
				}
			}

			// Destroy farthest tile
			tiles.Remove(farthest.Key);
			GameObject.Destroy(farthest.Value);
		}*/
	}

	protected void CreateTile(int x, int y)
	{
		// Check if tile already exists
		Tile tileKey = new Tile(x, y);
		if (!tiles.ContainsKey(tileKey))
		{
			// Convert to world pos
			Vector3 tileWorldPos = new Vector3(x * tileWidth, y * tileHeight, 0) + 0.5f * worldTilePrefab.transform.lossyScale;
			
			// Instantiate grid tile
			GameObject tile =
				(GameObject)GameObject.Instantiate(worldTilePrefab,
				                                   tileWorldPos, Quaternion.identity);
			tile.GetComponent<WorldTile>().Seed = seed;
			
			// Add tile to map
			tiles.Add(tileKey, tile);
		}
	}
}
