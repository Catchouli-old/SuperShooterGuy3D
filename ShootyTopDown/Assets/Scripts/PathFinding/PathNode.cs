using UnityEngine;
using System.Collections.Generic;

public abstract class PathNode
{
	// Interface
	public abstract Vector2 Position { get; }
	public abstract bool Accessible { get; }
	public abstract List<PathNode> AdjacentNodes { get; }
	public abstract List<PathNode> AdjacentNodesNoDiag { get; }
	public abstract override bool Equals(System.Object other);

	// Used in A* pathfinding
	public abstract float GScore { get; set; }
	public abstract float OverallScore { get; set; }
	public abstract PathNode Parent { get; set; }

	// A dummy implementation to make this warning go away
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
