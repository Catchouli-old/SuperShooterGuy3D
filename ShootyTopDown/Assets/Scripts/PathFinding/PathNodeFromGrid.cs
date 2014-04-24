using UnityEngine;
using System.Collections.Generic;
using System;

public class PathNodeFromGrid
	: PathNode
{
	public GridCell Cell { get { return cell; } }

	private CellGrid grid;
	private GridCell cell;
	
	private int GridWidth { get { return grid.Width; } }
	private int GridHeight { get { return grid.Height; } }
	
	private int x { get { return cell.x; } }
	private int y { get { return cell.y; } }

	public PathNodeFromGrid(CellGrid grid, GridCell cell)
	{
		this.grid = grid;
		this.cell = cell;
	}

	public override Vector2 Position
	{
		get
		{
			return new Vector2((float)cell.x, (float)cell.y);
		}
	}

	public override bool Accessible
	{
		get
		{
			return cell.Accessible;
		}
	}

	public override float GScore
	{
		get
		{
			return cell.gScore;
		}
		set
		{
			cell.gScore = value;
		}
	}
	
	public override float OverallScore
	{
		get
		{
			return cell.overallScore;
		}
		set
		{
			cell.overallScore = value;
		}
	}
	
	public override PathNode Parent
	{
		get
		{
			if (cell.parent == null)
				return null;

			return new PathNodeFromGrid(grid, cell.parent);
		}
		set
		{
			if (value == null)
			{
				cell.parent = null;
				return;
			}

			if (!(value is PathNodeFromGrid))
				throw new Exception();

			PathNodeFromGrid parent = ((PathNodeFromGrid)value);

			cell.parent = parent.cell;
		}
	}
	
	public override List<PathNode> AdjacentNodes
	{
		get
		{
			List<PathNode> adjacent = new List<PathNode>();
			
			for (int ix = x - 1; ix <= x+1; ++ix)
			{
				for (int iy = y-1; iy <= y+1; ++iy)
				{
					if (ix >= 0 && ix < GridWidth &&
					    iy >= 0 && iy < GridHeight)
					{
						PathNode node = grid.GetPathfindingNode(grid.Grid[ix, iy]);
						
						// Skip current node
						if (!node.Equals(this))
							adjacent.Add(node);
					}
				}
			}
			
			return adjacent;
		}
	}
	
	public override List<PathNode> AdjacentNodesNoDiag
	{
		get
		{
			List<PathNode> adjacent = new List<PathNode>();
			
			if (x-1 >= 0 && x-1 < GridWidth)
				adjacent.Add(grid.GetPathfindingNode(grid.Grid[x-1, y]));
			if (x+1 >= 0 && x+1 < GridWidth)
				adjacent.Add(grid.GetPathfindingNode(grid.Grid[x+1, y]));
			if (y-1 >= 0 && y-1 < GridHeight)
				adjacent.Add(grid.GetPathfindingNode(grid.Grid[x, y-1]));
			if (y+1 >= 0 && y+1 < GridHeight)
				adjacent.Add(grid.GetPathfindingNode(grid.Grid[x, y+1]));
			
			return adjacent;
		}
	}

	public override bool Equals(System.Object other)
	{
		if (other is PathNodeFromGrid)
		{
			PathNodeFromGrid otherGridNode = (PathNodeFromGrid)other;

			return otherGridNode.x == x && otherGridNode.y == y;
		}

		return false;
	}
	
	// A dummy implementation to make this warning go away
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}