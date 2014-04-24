using UnityEngine;
using System.Collections.Generic;

public static class MeshGenerator
{
	public struct WallDef
	{
		public Vector3 pos;
		public Vector3 normal;

		public bool outerWall;
	}

	private static List<WallDef> walls;
	public static List<WallDef> Walls
	{
		get
		{
			return walls;
		}
	}

	static MeshGenerator()
	{
		walls = new List<WallDef>();
	}

	public static Mesh GenerateMesh(Cell[,] grid)
	{
		int width = grid.GetLength(0);
		int height = grid.GetLength(1);

		// Mesh data
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> UVs = new List<Vector2>();
		List<int> triangles = new List<int>();
				
		// Generate geometry
		foreach (Cell cell in grid)
		{
			Vector3 cellPosition = new Vector3(cell.position.x, 0, cell.position.y);
			
			// Generate outer wall
			if (cell.position.x == 0)
			{
				// Create normal for face
				Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f);

				// Add it four times (there are four vertices per face..)
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				int index = vertices.Count;
				Vector3 wallPosition = cellPosition + new Vector3(-0.5f, 0.5f, 0);
				
				// Create light on wall
				AddWallToList(wallPosition, normal, true);
				
				// Generate vertices for wall
				vertices.Add(wallPosition + new Vector3(0,  0.5f,  0.5f));
				vertices.Add(wallPosition + new Vector3(0,  0.5f, -0.5f));
				vertices.Add(wallPosition + new Vector3(0, -0.5f,  0.5f));
				vertices.Add(wallPosition + new Vector3(0, -0.5f, -0.5f));
				
				// Generate UVs
				UVs.Add(new Vector2(0.5f, 0.5f));
				UVs.Add(new Vector2(1.0f, 0.5f));
				UVs.Add(new Vector2(0.5f, 1.0f));
				UVs.Add(new Vector2(1.0f, 1.0f));
				
				// Generate polygons
				triangles.Add(index);
				triangles.Add(index+1);
				triangles.Add(index+2);
				triangles.Add(index+2);
				triangles.Add(index+1);
				triangles.Add(index+3);
			}
			else if (cell.position.x == width-1)
			{
				// Create normal for face
				Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f);
				
				// Add it four times (there are four vertices per face..)
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				int index = vertices.Count;
				Vector3 wallPosition = cellPosition + new Vector3(0.5f, 0.5f, 0);
				
				// Create light on wall
				AddWallToList(wallPosition, normal, true);
				
				// Generate vertices for wall
				vertices.Add(wallPosition + new Vector3(0,  0.5f,  0.5f));
				vertices.Add(wallPosition + new Vector3(0,  0.5f, -0.5f));
				vertices.Add(wallPosition + new Vector3(0, -0.5f,  0.5f));
				vertices.Add(wallPosition + new Vector3(0, -0.5f, -0.5f));
				
				// Generate UVs
				UVs.Add(new Vector2(0.5f, 0.5f));
				UVs.Add(new Vector2(1.0f, 0.5f));
				UVs.Add(new Vector2(0.5f, 1.0f));
				UVs.Add(new Vector2(1.0f, 1.0f));
				
				// Generate polygons
				triangles.Add(index+3);
				triangles.Add(index+1);
				triangles.Add(index+2);
				triangles.Add(index+2);
				triangles.Add(index+1);
				triangles.Add(index);
			}
			
			if (cell.position.y == 0)
			{
				// Create normal for face
				Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f);
				
				// Add it four times (there are four vertices per face..)
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				int index = vertices.Count;
				Vector3 wallPosition = cellPosition + new Vector3(0, 0.5f, -0.5f);
				
				// Create light on wall
				AddWallToList(wallPosition, normal, true);
				
				// Generate vertices for wall
				vertices.Add(wallPosition + new Vector3(-0.5f,  0.5f, 0));
				vertices.Add(wallPosition + new Vector3( 0.5f,  0.5f, 0));
				vertices.Add(wallPosition + new Vector3(-0.5f, -0.5f, 0));
				vertices.Add(wallPosition + new Vector3( 0.5f, -0.5f, 0));
				
				// Generate UVs
				UVs.Add(new Vector2(0.5f, 0.5f));
				UVs.Add(new Vector2(1.0f, 0.5f));
				UVs.Add(new Vector2(0.5f, 1.0f));
				UVs.Add(new Vector2(1.0f, 1.0f));
				
				// Generate polygons
				triangles.Add(index);
				triangles.Add(index+1);
				triangles.Add(index+2);
				triangles.Add(index+2);
				triangles.Add(index+1);
				triangles.Add(index+3);
				
			}
			else if (cell.position.y == height-1)
			{
				// Create normal for face
				Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f);
				
				// Add it four times (there are four vertices per face..)
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				int index = vertices.Count;
				Vector3 wallPosition = cellPosition + new Vector3(0, 0.5f, 0.5f);
				
				// Create light on wall
				AddWallToList(wallPosition, normal, true);
				
				// Generate vertices for wall
				vertices.Add(wallPosition + new Vector3(-0.5f,  0.5f, 0));
				vertices.Add(wallPosition + new Vector3( 0.5f,  0.5f, 0));
				vertices.Add(wallPosition + new Vector3(-0.5f, -0.5f, 0));
				vertices.Add(wallPosition + new Vector3( 0.5f, -0.5f, 0));
				
				// Generate UVs
				UVs.Add(new Vector2(0.5f, 0.5f));
				UVs.Add(new Vector2(1.0f, 0.5f));
				UVs.Add(new Vector2(0.5f, 1.0f));
				UVs.Add(new Vector2(1.0f, 1.0f));
				
				// Generate polygons
				triangles.Add(index+3);
				triangles.Add(index+1);
				triangles.Add(index+2);
				triangles.Add(index+2);
				triangles.Add(index+1);
				triangles.Add(index);
			}
			
			// If this isn't a walkable cell
			if (!cell.visited)
			{
				// Create normal for face
				Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f);
				
				// Add it four times (there are four vertices per face..)
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);
				normals.Add(normal);

				// Generate ceiling
				int index = vertices.Count;
				Vector3 ceilingPosition = cellPosition + new Vector3(0, 1.0f, 0);
				
				// Generate vertices for wall
				vertices.Add(ceilingPosition + new Vector3( 0.5f, 0,  0.5f));
				vertices.Add(ceilingPosition + new Vector3( 0.5f, 0, -0.5f));
				vertices.Add(ceilingPosition + new Vector3(-0.5f, 0,  0.5f));
				vertices.Add(ceilingPosition + new Vector3(-0.5f, 0, -0.5f));
				
				// Generate UVs
				UVs.Add(new Vector2(0.5f, 0.5f));
				UVs.Add(new Vector2(1.0f, 0.5f));
				UVs.Add(new Vector2(0.5f, 1.0f));
				UVs.Add(new Vector2(1.0f, 1.0f));
				
				// Generate polygons
				triangles.Add(index);
				triangles.Add(index+1);
				triangles.Add(index+2);
				triangles.Add(index+2);
				triangles.Add(index+1);
				triangles.Add(index+3);
			}
			else
			{
				// Create normal for face
				Vector3 normalfloor = new Vector3(0.0f, 1.0f, 0.0f);
				
				// Add it four times (there are four vertices per face..)
				normals.Add(normalfloor);
				normals.Add(normalfloor);
				normals.Add(normalfloor);
				normals.Add(normalfloor);

				// Generate floor
				if (cell.generateFloor)
				{
					int index = vertices.Count;
					Vector3 floorPosition = cellPosition + new Vector3(0, 0.0f, 0);
					
					// Generate vertices for floor
					vertices.Add(floorPosition + new Vector3( 0.5f, 0,  0.5f));
					vertices.Add(floorPosition + new Vector3( 0.5f, 0, -0.5f));
					vertices.Add(floorPosition + new Vector3(-0.5f, 0,  0.5f));
					vertices.Add(floorPosition + new Vector3(-0.5f, 0, -0.5f));
					
					// Generate UVs
					UVs.Add(new Vector2(0, 1.0f));
					UVs.Add(new Vector2(0.5f, 1.0f));
					UVs.Add(new Vector2(0, 0.5f));
					UVs.Add(new Vector2(0.5f, 0.5f));
					
					// Generate polygons
					triangles.Add(index);
					triangles.Add(index+1);
					triangles.Add(index+2);
					triangles.Add(index+2);
					triangles.Add(index+1);
					triangles.Add(index+3);
				}
				
				// Generate walls
				if (cell.position.x == 0 || !grid[cell.position.x-1, cell.position.y].visited)
				{
					// Create normal for face
					Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f);
					
					// Add it four times (there are four vertices per face..)
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);

					int index = vertices.Count;
					Vector3 wallPosition = cellPosition + new Vector3(-0.5f, 0.5f, 0);

					// Create light on wall
					AddWallToList(wallPosition, normal, false);
					
					// Generate vertices for wall
					vertices.Add(wallPosition + new Vector3(0,  0.5f,  0.5f));
					vertices.Add(wallPosition + new Vector3(0,  0.5f, -0.5f));
					vertices.Add(wallPosition + new Vector3(0, -0.5f,  0.5f));
					vertices.Add(wallPosition + new Vector3(0, -0.5f, -0.5f));
					
					// Generate UVs
					UVs.Add(new Vector2(0.5f, 0.5f));
					UVs.Add(new Vector2(1.0f, 0.5f));
					UVs.Add(new Vector2(0.5f, 1.0f));
					UVs.Add(new Vector2(1.0f, 1.0f));
					
					// Generate polygons
					triangles.Add(index+3);
					triangles.Add(index+1);
					triangles.Add(index+2);
					triangles.Add(index+2);
					triangles.Add(index+1);
					triangles.Add(index);
				}
				
				if (cell.position.x == width-1 || !grid[cell.position.x+1, cell.position.y].visited)
				{
					// Create normal for face
					Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f);
					
					// Add it four times (there are four vertices per face..)
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);

					int index = vertices.Count;
					Vector3 wallPosition = cellPosition + new Vector3(0.5f, 0.5f, 0);
					
					// Create light on wall
					AddWallToList(wallPosition, normal, false);
					
					// Generate vertices for wall
					vertices.Add(wallPosition + new Vector3(0,  0.5f,  0.5f));
					vertices.Add(wallPosition + new Vector3(0,  0.5f, -0.5f));
					vertices.Add(wallPosition + new Vector3(0, -0.5f,  0.5f));
					vertices.Add(wallPosition + new Vector3(0, -0.5f, -0.5f));
					
					// Generate UVs
					UVs.Add(new Vector2(0.5f, 0.5f));
					UVs.Add(new Vector2(1.0f, 0.5f));
					UVs.Add(new Vector2(0.5f, 1.0f));
					UVs.Add(new Vector2(1.0f, 1.0f));
					
					// Generate polygons
					triangles.Add(index);
					triangles.Add(index+1);
					triangles.Add(index+2);
					triangles.Add(index+2);
					triangles.Add(index+1);
					triangles.Add(index+3);
				}
				
				if (cell.position.y == 0 || !grid[cell.position.x, cell.position.y-1].visited)
				{
					// Create normal for face
					Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f);
					
					// Add it four times (there are four vertices per face..)
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);

					int index = vertices.Count;
					Vector3 wallPosition = cellPosition + new Vector3(0, 0.5f, -0.5f);
					
					// Create light on wall
					AddWallToList(wallPosition, normal, false);
					
					// Generate vertices for wall
					vertices.Add(wallPosition + new Vector3(-0.5f,  0.5f, 0));
					vertices.Add(wallPosition + new Vector3( 0.5f,  0.5f, 0));
					vertices.Add(wallPosition + new Vector3(-0.5f, -0.5f, 0));
					vertices.Add(wallPosition + new Vector3( 0.5f, -0.5f, 0));
					
					// Generate UVs
					UVs.Add(new Vector2(0.5f, 0.5f));
					UVs.Add(new Vector2(1.0f, 0.5f));
					UVs.Add(new Vector2(0.5f, 1.0f));
					UVs.Add(new Vector2(1.0f, 1.0f));
					
					// Generate polygons
					triangles.Add(index+3);
					triangles.Add(index+1);
					triangles.Add(index+2);
					triangles.Add(index+2);
					triangles.Add(index+1);
					triangles.Add(index);
				}
				
				if (cell.position.y == height-1 || !grid[cell.position.x, cell.position.y+1].visited)
				{
					// Create normal for face
					Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f);
					
					// Add it four times (there are four vertices per face..)
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);
					normals.Add(normal);

					int index = vertices.Count;
					Vector3 wallPosition = cellPosition + new Vector3(0, 0.5f, 0.5f);
					
					// Create light on wall
					AddWallToList(wallPosition, normal, false);
					
					// Generate vertices for wall
					vertices.Add(wallPosition + new Vector3(-0.5f,  0.5f, 0));
					vertices.Add(wallPosition + new Vector3( 0.5f,  0.5f, 0));
					vertices.Add(wallPosition + new Vector3(-0.5f, -0.5f, 0));
					vertices.Add(wallPosition + new Vector3( 0.5f, -0.5f, 0));
					
					// Generate UVs
					UVs.Add(new Vector2(0.5f, 0.5f));
					UVs.Add(new Vector2(1.0f, 0.5f));
					UVs.Add(new Vector2(0.5f, 1.0f));
					UVs.Add(new Vector2(1.0f, 1.0f));
					
					// Generate polygons
					triangles.Add(index);
					triangles.Add(index+1);
					triangles.Add(index+2);
					triangles.Add(index+2);
					triangles.Add(index+1);
					triangles.Add(index+3);
				}
			}
		}
		
		// Update mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = UVs.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.normals = normals.ToArray();

		return mesh;
	}

	private static void AddWallToList(Vector3 pos, Vector3 wallNormal, bool outer)
	{
		WallDef wall;
		wall.pos = pos;
		wall.normal = wallNormal;
		wall.outerWall = outer;

		walls.Add(wall);
	}
}