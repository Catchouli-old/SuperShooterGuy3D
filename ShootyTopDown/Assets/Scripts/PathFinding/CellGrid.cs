using UnityEngine;
using System.Collections.Generic;

public partial class CellGrid
{
	public GridCell[,] Grid { get { return grid; } }
	public int Width { get { return grid.GetLength(0); } }
	public int Height { get { return grid.GetLength(1); } }

	private GridCell[,] grid;

	private Vector3 offset;
	private Vector3 scale;
	private Vector3 invScale;

	public CellGrid(GridCell[,] grid, Transform transform)
	{
		this.grid = grid;

		offset = transform.position;

		scale = new Vector3(transform.lossyScale.x,
		                                     transform.lossyScale.z, 1);

		invScale = new Vector3(1.0f / transform.lossyScale.x,
		                               1.0f / transform.lossyScale.z, 1);
	}

	public PathNode GetPathfindingNode(GridCell cell)
	{
		return new PathNodeFromGrid(this, cell);
	}
	
	public List<GridCell> FindPath(GridCell start, GridCell end)
	{
		List<PathNode> nodes = PathFinding.FindPath(
			GetPathfindingNode(start),
			GetPathfindingNode(end));
		
		List<GridCell> cells = new List<GridCell>();
		
		foreach (PathNode node in nodes)
		{
			cells.Add(((PathNodeFromGrid)node).Cell);
		}
		
		return cells;
	}
	
	public List<Vector3> FindPathWorld(GridCell start, GridCell end)
	{
		List<PathNode> nodes = PathFinding.FindPath(
			GetPathfindingNode(start),
			GetPathfindingNode(end));
		
		List<Vector3> path = new List<Vector3>();
		
		foreach (PathNode node in nodes)
		{
			path.Add(GetCellPos(((PathNodeFromGrid)node).Cell));
		}
		
		return path;
	}

	public Vector3 GetCellPos(GridCell cell)
	{
		Vector3 worldPos = new Vector3((float)cell.x,
		                               (float)cell.y);
		worldPos.Scale(scale);
		worldPos += offset;
		
		return worldPos;
	}
	
	public bool IsInGrid(Vector3 pos)
	{
		Point cellPos = GetIdxFromPos(pos);
		
		return (cellPos.x >= 0 && cellPos.x < grid.GetLength(0) &&
		        cellPos.y >= 0 && cellPos.y < grid.GetLength(1));
	}
	
	public GridCell GetCellAtPos(Vector3 pos)
	{
		Point cellPos = GetIdxFromPos(pos);
		
		return grid[cellPos.x, cellPos.y];
	}
	
	private Point GetIdxFromPos(Vector3 pos)
	{
		float cellSizeX = 1.0f;
		float cellSizeY = 1.0f;
		
		float halfCellSizeX = 0.5f * cellSizeX;
		float halfCellSizeY = 0.5f * cellSizeY;
		
		pos -= offset;
		pos.Scale(1.0f * invScale);
		
		int x = (int)Mathf.Floor((pos.x + halfCellSizeX) / cellSizeX);
		int y = (int)Mathf.Floor((pos.y + halfCellSizeY) / cellSizeY);
		
		return new Point(x, y);
	}
}
