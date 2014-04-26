using UnityEngine;
using System;

public class Cell
{
	public Point position;
	
	public bool visited;
	
	public bool key;
	public bool door;
	public bool end;
	public bool deadend;
	
	public Cell next;
	
	public bool generateFloor;

	public float gScore;
	public float overallScore;
	public Cell parent;
	
	public Cell(int x, int y)
	{
		Initialise(x, y);
	}

	public Cell(Cell oldCell)
	{
		deadend = oldCell.deadend;
		door = oldCell.door;
		end = oldCell.end;
		generateFloor = oldCell.generateFloor;
		gScore = oldCell.gScore;
		key = oldCell.key;
		next = oldCell.next;
		overallScore = oldCell.overallScore;
		parent = oldCell.parent;
		position = oldCell.position;
		visited = oldCell.visited;
	}
	
	public Vector3 GetDirection(Cell other)
	{
		Vector3 thisPos = new Vector3(position.x, 0.0f, position.y);
		Vector3 otherPos = new Vector3(other.position.x, 0.0f, other.position.y);
		
		// Calculate direction
		Vector3 difference = otherPos - thisPos;
		Vector3 direction = difference.normalized;
		
		return direction;
	}
	
	private void Initialise(int x, int y)
	{
		position = new Point(x, y);
		
		next = null;
		
		visited = false;
		
		key = false;
		door = false;
		end = false;
		deadend = false;
		
		generateFloor = true;
	}
}