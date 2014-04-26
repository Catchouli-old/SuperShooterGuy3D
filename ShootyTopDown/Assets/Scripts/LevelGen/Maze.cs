using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Maze
	: MonoBehaviour
{
	public enum SeedMode
	{
		TIME,
		PRESET
	}
	
	public enum StartMode
	{
		RANDOM,
		PRESET
	}

	public enum Mode
	{
		PERFECT,
		CREATELOOPS
	}
	
	public class DoesntWorkItsFuckedException : Exception {}

	private const int SOLUTION_MAX_ITERATIONS = 1000;
	
	public int width = 5;
	public int height = 5;
	
	public int seed = 0;
	public SeedMode seedMode = SeedMode.TIME;
	
	public StartMode startMode;
	public Point genStart = new Point(1, 1);

	public Mode generationMode = Mode.PERFECT;
	
	public Cell[,] Grid { get { return grid; } }
	
	public List<Cell> DeadEnds { get { return deadends; } }
	public List<Cell> OpenCells { get { return opencells; } }
	
	public bool autoPlay = false;
	
	private Cell[,] grid;
	
	private List<Cell> deadends;
	private List<Cell> opencells;
	
	private Mesh mesh;
	
	private System.Random generator;

	public void Start()
	{
		// Seed random number generator
		if (seedMode == SeedMode.TIME)
			generator = new System.Random();
		else
			generator = new System.Random(seed);
		
		// Pass over reference to rng so it uses the same one
		Extensions.generator = generator;
	}

	public void Regenerate()
	{
		Regenerate(generationMode);
	}
	
	public void Regenerate(Mode mode)
	{
		// Set start point
		if (startMode == StartMode.RANDOM)
		{
			genStart.x = generator.Next(0, (width)/2 - 1) * 2 + 1;
			genStart.y = generator.Next(0, (height)/2 - 1) * 2 + 1;
		}
		
		// Stuff
		grid = new Cell[width, height];
		deadends = new List<Cell>();
		opencells = new List<Cell>();
		
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				grid[x, y] = new Cell(x, y);
			}
		}
		
		// Generate maze
		GenerateDepthFirst();

		if (mode == Mode.CREATELOOPS)
		{
			foreach (Cell cell in deadends)
			{
				// Find neighbours
				List<Cell> neighbours = NeighboursPastWalls(this, cell.position);
				neighbours.Shuffle();

				// Go through neighbours at random
				// until we find an open one
				foreach (Cell neighbour in neighbours)
				{
					if (neighbour.visited)
					{
						// Carve a path to first open cell
						Point wallPos = new Point((cell.position.x + neighbour.position.x) / 2,
						                          (cell.position.y + neighbour.position.y) / 2);

						grid[wallPos.x, wallPos.y].visited = true;

						break;
					}
				}
			}
		}

		// Add cells to open list
		foreach (Cell cell in Grid)
		{
			if (cell.visited)
			{
				OpenCells.Add(cell);
			}
		}
	}
	
	public void GenerateDepthFirst()
	{
		// Start generation
		Maze.GenerateDepthFirstWork(this);
	}
	
	public static void GenerateDepthFirstWork(Maze maze)
	{
		// Set the start cell
		maze.grid[maze.genStart.x, maze.genStart.y].end = true;

		maze.deadends.Add(maze.grid[maze.genStart.x, maze.genStart.y]);
		
		// Start generation
		Maze.GenerateDepthFirstRecursive(maze, maze.grid[maze.genStart.x, maze.genStart.y]);
	}
	
	static void GenerateDepthFirstRecursive(Maze maze, Cell cell)
	{
		// Mark as visited
		cell.visited = true;
		
		// Set dead end to true until we move to a neighbour
		cell.deadend = true;
		maze.deadends.Add(cell);
		
		// Get list of neighbours
		List<Cell> neighbours = NeighboursPastWalls(maze, cell.position);
		
		// Randomised their order
		neighbours.Shuffle();
		
		// Iterate through them
		foreach (Cell neighbour in neighbours)
		{
			// If we haven't already visited this cell
			if (!neighbour.visited)
			{
				// This can't possibly be an end
				cell.deadend = false;
				neighbour.next = cell;
				maze.deadends.Remove(cell);
				
				// Break down wall in between
				int wallX = cell.position.x + (neighbour.position.x - cell.position.x) / 2;
				int wallY = cell.position.y + (neighbour.position.y - cell.position.y) / 2;
				maze.grid[wallX, wallY].visited = true;
				
				// Recurse on this cell
				GenerateDepthFirstRecursive(maze, neighbour);
			}
		}
	}
	
	public static List<Cell> Neighbours(Maze maze, Point point)
	{
		List<Cell> neighbours = new List<Cell>();
		
		if (point.y > 1)
		{
			neighbours.Add(maze.Grid[point.x, point.y-1]);
		}
		
		if (point.y < maze.height-2)
		{
			neighbours.Add(maze.Grid[point.x, point.y+1]);
		}
		
		if (point.x > 1)
		{
			neighbours.Add(maze.Grid[point.x-1, point.y]);
		}
		
		if (point.x < maze.width-2)
		{
			neighbours.Add(maze.Grid[point.x+1, point.y]);
		}
		
		return neighbours;
	}
	
	public static List<Cell> NeighboursPastWalls(Maze maze, Point point)
	{
		List<Cell> neighbours = new List<Cell>();
		
		if (point.y > 2)
		{
			neighbours.Add(maze.Grid[point.x, point.y-2]);
		}
		
		if (point.y < maze.height-3)
		{
			neighbours.Add(maze.Grid[point.x, point.y+2]);
		}
		
		if (point.x > 2)
		{
			neighbours.Add(maze.Grid[point.x-2, point.y]);
		}
		
		if (point.x < maze.width-3)
		{
			neighbours.Add(maze.Grid[point.x+2, point.y]);
		}
		
		return neighbours;
	}
}
