using UnityEngine;
using System.Collections;

public class WorldTile : MonoBehaviour
{
	private const float CELL_WIDTH = 2.0f;
	private const float CELL_HEIGHT = 2.0f;
	
	public int width = 10;
	public int height = 10;

	public Vector3 noiseScale = Vector3.one;

	public Material cubeMaterial;

	public float Seed { get; set; }

	private float[,] grid;
	private bool[,] bm;

	private Texture2D tex;

	private void Start()
	{
		// Create texture
		bm = new bool[width, height];
		tex = new Texture2D(width, height);
		tex.wrapMode = TextureWrapMode.Clamp;

		// Generate world
		GenWorld();

		// Create mesh
		CreateMesh();

		// Set texture
		tex.Apply();
		renderer.material.SetTexture("_MainTex", tex);
	}

	private void GenWorld()
	{
		// Create rotation for noise (so they are less likely to be integers)
		Quaternion rot = Quaternion.Euler(45, 45, 45);

		// Create new grid
		grid = new float[width, height];

		/*Vector3 initialArgs =
			new Vector3((transform.position.x / transform.lossyScale.x) * (float)width,
			            (transform.position.y / transform.lossyScale.y) * (float)height,
			            seed);

		Vector3 diff = new Vector3(1, 1, 0);*/

		Vector3 initialArgs = transform.position;
		initialArgs.z = Seed;

		Vector3 diff = new Vector3(transform.lossyScale.x / (float)width,
		                           transform.lossyScale.y / (float)height,
		                           0);

		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				// Initialise perlin arguments
				//Vector3 perlinArgs =
				//	new Vector3(x / (float)width, y / (float)height, seed);
				/*Vector3 perlinArgs =
					new Vector3(x, y, seed);
				
				perlinArgs.x += (transform.position.x / transform.lossyScale.x) * (float)width;
				perlinArgs.y += (transform.position.y / transform.lossyScale.y) * (float)height;*/

				Vector3 perlinArgs = initialArgs + new Vector3(diff.x * x, diff.y * y, 0);

				perlinArgs.Scale(noiseScale);

				// Rotate perlin args so they're unlikely to be integers
				perlinArgs = rot * perlinArgs;

				// Generate perlin noise
				float perlin = ImprovedPerlin.Noise(perlinArgs.x, perlinArgs.y, perlinArgs.z);

				// Set grid values to noise
				grid[x, y] = perlin;
			}
		}
	}

	private void CreateMesh()
	{
		// Create cubes for grid
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				float worldX = x * CELL_WIDTH;
				float worldY = y * CELL_HEIGHT;

				float gridVal = grid[x, y];

				Color color = Color.white;

				if (gridVal < 0.1f)
				{
					color.a = 0.0f;
					bm[x, y] = true;
				}
				else
				{
					bm[x, y] = false;
				}

				tex.SetPixel(x, y, color);

				//if (grid[x, y] > 0.1f)
				{
					/*GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = new Vector3(worldX, worldY, 0);
					cube.transform.localScale = new Vector3(CELL_WIDTH, CELL_HEIGHT, 1.0f);
					cube.renderer.material = cubeMaterial;

					// Set alpha
					Color color = cube.renderer.material.color;
					color.a = grid[x, y];
					cube.renderer.material.color = color;*/
				}
			}
		}
	}
}
