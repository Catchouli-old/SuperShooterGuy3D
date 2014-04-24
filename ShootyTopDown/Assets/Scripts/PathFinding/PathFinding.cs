using UnityEngine;
using System.Collections.Generic;

class PathFinding
{	
	// A* Pathfinding
	public static List<PathNode> FindPath(PathNode start, PathNode end)
	{
		// TODO: diagonal cost not working right??}{???
		const float STRAIGHT_COST = 1.0f;
		const float DIAG_COST = 1.41421356237f;
		
		List<PathNode> open = new List<PathNode>();
		List<PathNode> closed = new List<PathNode>();
		
		// Return immediately if we're already at the end
		// or the cell we're on is blocked
		if (start.Equals(end))
		{
			List<PathNode> nodes = new List<PathNode>();
			nodes.Add(start);
			return nodes;
		}
		
		// Add starting cell to open list
		start.Parent = null;
		start.GScore = 0;
		start.OverallScore = 0;
		open.Add(start);

		// Set end parent to null so we know afterwards if we found it
		end.Parent = null;
				
		// Loop while open list has cells
		while (open.Count > 0)
		{
			// Find node in open list with lowest cost
			PathNode parent = open[0];
			
			foreach (PathNode node in open)
			{
				if (node.OverallScore < parent.OverallScore)
					parent = node;
			}
			
			// Remove this node from the open list and add it to the closed list
			open.Remove(parent);
			closed.Add(parent);

			// Check if we've found the end
			if (parent == end)
				break;
			
			// Check adjacent nodes
			foreach (PathNode node in parent.AdjacentNodes)
			{
				// Ignore blocked nodes
				if (!node.Accessible)
					continue;

				if (closed.Contains(node))
					continue;
				
				// Calculate cost to get to this node from current node
				float xdiff = Mathf.Abs(node.Position.x - start.Position.x);
				float ydiff = Mathf.Abs(node.Position.y - start.Position.y);
				float manhattenDistance = xdiff + ydiff;

				float G = parent.GScore;

				if (manhattenDistance == 1.0f)
				    G += STRAIGHT_COST;
			    else
					G += DIAG_COST;
				
				// Calculate heuristic distance from this cell to the end
				xdiff = Mathf.Abs(node.Position.x - end.Position.x);
				ydiff = Mathf.Abs(node.Position.x - end.Position.x);
				manhattenDistance = xdiff + ydiff;
				
				float H = manhattenDistance * STRAIGHT_COST;
				
				// Calculate score
				float F = G + H;
				
				// If this node is already on the open list
				if (open.Contains(node))
				{
					// If this path is better, replace it
					if (G < node.GScore)
					{
						node.GScore = G;
						node.OverallScore = F;
						node.Parent = parent;
					}
				}
				else
				{
					node.GScore = G;
					node.OverallScore = F;
					node.Parent = parent;
				
					// Add this node to the open list
					open.Add(node);
				}
			}
		}

		// Return an empty path if we couldn't find the end
		if (end.Parent == null)
			return new List<PathNode>();

		// Trace the path back
		List<PathNode> path = new List<PathNode>();

		PathNode currentNode = end;
		while (!currentNode.Equals(start))
		{
			path.Add(currentNode);
			currentNode = currentNode.Parent;
		}
		path.Add(currentNode);

		path.Reverse();

		return path;
	}
}