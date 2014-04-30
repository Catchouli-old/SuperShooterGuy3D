#define NPC_DEBUG
//#define DEBUG_FOV_RAYCASTS
//#define DEBUG_PLAYER_RAYCASTS
//#define DEBUG_COVER_RAYCASTS
//#define FAST_MODE

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NonplayerCharacter
	: CharacterBase
{
	public enum State
	{
		MAP_LEVEL,
		PATROL_LEVEL,
		ATTACK_PLAYER,
		LOOK_FOR_PLAYER
	}

	// Weapon firing speeds per second
	private const float PISTOL_FIRE_RATE = 3;
	private const float AUTOMATIC_FIRE_RATE = 1000.0f;	// The automatic can just be held down
	private const float SHOTGUN_FIRE_RATE = 1;
	private const float RAILGUN_FIRE_RATE = 1000.0f; // The railgun is super slow already!!

	// Movement
	private const float DEFAULT_MOVE_SPEED = 0.5f;

	// Visibility
	private const int FOV_STEPS = 30;
	private const float FOV = 60.0f;

	// Behaviour
	private const float LOOK_PLAYER_JITTER = 15;
	private const float PLAYER_PATH_REGEN_TIME = 1.0f;

	public static CellState[,] CellStates { get { return cellStates; } }
	public static GridCell[,] PathFindingGrid { get { return pathFindingGrid; } }
	
	public GameObject target;
	
	public LayerMask levelLayer;
	public LayerMask playerLayer;
	public LayerMask powerupLayer;
	
	private static CellGrid cellGrid;
	private static GridCell[,] pathFindingGrid;
	private static CellState[,] cellStates;
	
	private static int gridWidth, gridHeight;
	
	private static Texture2D tex;
	private static Texture2D tex2;
	
	private LineRenderer lineRenderer;
	
	private List<PathNode> currentPath;
	private static List<PathNode> currentTargets;
	
	private State currentState = State.MAP_LEVEL;
	private State previousState = State.MAP_LEVEL;
	private bool stateJustChanged = true;

	private Vector3 movementDir = Vector3.zero;
	private float targetMovementSpeed = DEFAULT_MOVE_SPEED;
	
	private Vector3 lastPlayerPos;
	private float lastPlayerTime;

	private bool gotPlayerPos;
	private Vector3 playerPos;
	
	private PathNode currentNode;
	private PathNode targetNode;

	private float stateStartTime = 0;

	private List<Cell> deadEnds;

	private float lastTimePlayerPathCalculated = -PLAYER_PATH_REGEN_TIME;

	private float lastTimeFired = -1000.0f;
	private float currentWeaponFireRate = PISTOL_FIRE_RATE;

	private int lineVertices = 0;

	static NonplayerCharacter()
	{
		// Current pathfinding targets so that they don't pick the same ones
		currentTargets = new List<PathNode>();
	}

	// Used to reset the pathfinding when the level is regenerated
	public static void ResetPathfinding()
	{		
		gridWidth = LevelGen.CellGrid.Width;
		gridHeight = LevelGen.CellGrid.Height;
		
		// Allocate grids
		cellStates = new CellState[gridWidth, gridHeight];
		pathFindingGrid = new GridCell[gridWidth, gridHeight];
		
		// Initialise state grid
		for (int x = 0; x < gridWidth; ++x)
			for (int y = 0; y < gridHeight; ++y)
		{
			cellStates[x, y] = CellState.UNKNOWN;
		}
		
		// Initialise pathfinding grid
		for (int x = 0; x < gridWidth; ++x)
			for (int y = 0; y < gridHeight; ++y)
		{
			pathFindingGrid[x, y] = new GridCell(x, y, false);
		}
		
		cellGrid = new CellGrid(pathFindingGrid, LevelGen.Instance.transform, LevelGen.MAZE_RESOLUTION);
		
		currentTargets.Clear();
	}
	
	// Damage taken event handler
	public override void DamageTaken()
	{
		base.DamageTaken();

		// Look for player if damage taken and player position unknown
		if (currentState != State.ATTACK_PLAYER)
		{
			SwitchState(State.LOOK_FOR_PLAYER);
			currentPath = null;
		}
	}

	// Initialise some agent-specific pathfinding stuff
	protected override void Start()
	{
		// Run base start method
		base.Start();

		// Get deadends
		deadEnds = ((Maze)GameObject.FindObjectOfType(typeof(Maze))).DeadEnds;
		
		// Add initial position to grid
		if (LevelGen.CellGrid.IsInGrid(transform.position))
		{
			GridCell curCell = LevelGen.CellGrid.GetCellAtPos(transform.position);
			int curX = curCell.x;
			int curY = curCell.y;
			
			cellStates[curX, curY] = CellState.CLEAR;
			pathFindingGrid[curX, curY].Accessible = true;
		}
		
#if NPC_DEBUG
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetWidth(0.1f, 0.1f);
#endif
	}

	// Fire weapon - called by state handlers
	protected override void Fire()
	{
		if (currentWeaponFireRate <= 0)
			return;

		float currentWeaponFireMinTime = 1.0f / currentWeaponFireRate;

		if (Time.time - lastTimeFired >= currentWeaponFireMinTime)
		{
			// Fire weapon
			base.Fire();

			lastTimeFired = Time.time;
		}
	}

	// Update min weapon time when weapon is switched
	public override void SwitchedWeapon()
	{
		switch (CurrentWeapon)
		{
		case WeaponType.PISTOL:
			currentWeaponFireRate = PISTOL_FIRE_RATE;
			break;
		case WeaponType.AUTOMATIC:
			currentWeaponFireRate = AUTOMATIC_FIRE_RATE;
			break;
		case WeaponType.SHOTGUN:
			currentWeaponFireRate = SHOTGUN_FIRE_RATE;
			break;
		case WeaponType.RAILGUN:
			currentWeaponFireRate = RAILGUN_FIRE_RATE;
			break;
		}
	}

	// Called to switch current state
	protected void SwitchState(State newState)
	{
		if (newState != currentState)
		{
			previousState = currentState;
			currentState = newState;
			stateJustChanged = true;
			stateStartTime = Time.time;
		}
	}

	// Handle states and pathfinding
	protected override void Update()
	{
		Debug.Log("Current state: " + currentState);

		lineVertices = 0;

		// Call base update method
		base.Update();
		
		// Get current node
		GridCell currentCell = cellGrid.GetCellAtPos(transform.position);
		PathNode oldNode = currentNode;
		currentNode = cellGrid.GetPathfindingNode(currentCell);

		// Common update stuff
		// Do visibility testing
		TestVisibility();

		// Look for powerups and collect them if we can see them
		Vector3 powerupPos;
		if (currentState != State.ATTACK_PLAYER && currentState != State.LOOK_FOR_PLAYER)
		if (CanSeeObjectOnLayer(FacingDirection, powerupLayer, "Powerup", out powerupPos))
		{
			Vector3 dir = (powerupPos - transform.position).normalized;

			Move(dir);
			TurnTowards(dir);
		}

		// Set default movement speed (later gets replaced in state handlers)
		targetMovementSpeed = DEFAULT_MOVE_SPEED;

		// Handle states
		// Return early if the state handler returns false
		switch (currentState)
		{
		case State.MAP_LEVEL:

			if (!MapLevel())
				return;

			break;

		case State.PATROL_LEVEL:
			
			if (!PatrolLevel())
				return;

			break;
			
			
		case State.ATTACK_PLAYER:
			
			if (!AttackPlayer())
				return;
			
			break;
			
		case State.LOOK_FOR_PLAYER:
			
			if (!LookForPlayer())
				return;
			
			break;

		default:
			Debug.LogError("NonplayerCharacter: invalid state reached");
			return;
		}
		
		// Follow path if there is one
		if (currentPath != null)
		{
			// Remove current node from path
			if (currentPath.Contains(currentNode))
				currentPath.Remove(currentNode);
			
			// Clear path if empty
			if (currentPath.Count == 0)
			{
				currentPath = null;
			}
			else
			{
				// Get last node in path
				PathNode lastNode = currentPath.Last();
				
				// Walk the path if it's still not null
				if (currentPath != null)
				{
					// Walk path if not empty
					PathNode nextNode = currentPath[0];
					
					// Get current state of cell
					CellState state = cellStates[(int)nextNode.Position.x, (int)nextNode.Position.y];
					
					// If blocked, clear the path
					if (state == CellState.BLOCKED)
					{
						currentPath = null;
					}
					else
					{
						// Move towards next node in path
						Vector3 dir = (nextNode.Position - currentNode.Position).normalized;

						movementDir = dir;
						Move(dir * targetMovementSpeed);
					}
				}
			}
		}

		// Reset state just changed flag
		stateJustChanged = false;
	}

	// State handlers
	protected bool MapLevel()
	{
		// Turn towards movemment direction
		TurnTowards(movementDir);

		// Check if we can see the player and switch to chase player if so
		if (CanSeeObjectOnLayer(FacingDirection, playerLayer, "Player"))
		{
			SwitchState(State.ATTACK_PLAYER);
			currentPath = null;
			return false;
		}

		// If there's a current path, check if it's still interesting (has an unknown adjacent to it)
		if (currentPath != null)
		{
			PathNode end = currentPath.Last();

			bool foundUnknown = false;
			foreach (PathNode node in end.AdjacentNodes)
			{
				CellState state = cellStates[(int)node.Position.x, (int)node.Position.y];

				if (state == CellState.UNKNOWN)
				{
					foundUnknown = true;
					break;
				}
			}

			// If there are no unknowns, clear the path
			if (!foundUnknown)
				currentPath = null;
		}

		// If there's no current path, path find to a blue square
		if (currentPath == null)
		{
			currentPath = FindNearestPathToUnknown(currentNode);
			
			// If this path only has one entry (the current cell),
			// we're right next to an unknown,
			// so we should pathfind to that instead
			if (currentPath.Count == 1)
			{
				currentPath.Clear();
				
				foreach (PathNode node in currentNode.AdjacentNodesNoDiag)
				{
					CellState state = cellStates[(int)node.Position.x, (int)node.Position.y];
					
					if (state == CellState.UNKNOWN)
					{
						currentPath.Add(node);
						break;
					}
				}
			}
			// If there are no unknowns left (or none reachable)
			else if (currentPath.Count == 0)
			{
				// Switch state to patroling
				SwitchState(State.PATROL_LEVEL);
			}
			else
			{
				// Get last node in path
				PathNode lastNode = currentPath.Last();
				
				// Add it to targets
				currentTargets.Add(lastNode);
			}
		}

		return true;
	}
	
	protected bool PatrolLevel()
	{
		// Turn towards movemment direction
		TurnTowards(movementDir);

		// Check if we can see the player and switch to chase player if so
		if (CanSeeObjectOnLayer(FacingDirection, playerLayer, "Player"))
		{
			SwitchState(State.ATTACK_PLAYER);
			currentPath = null;
			return false;
		}

		if (targetNode != null)
		{
			if (targetNode.Equals(currentNode))
				targetNode = null;
		}

		if (currentPath == null)
		{
			// Find a random walkable tile to path find to
			GridCell gridCell = cellGrid.Grid[0, 0];

			int i = 0;
			const int max_iter = 5;
			while (!gridCell.Accessible)
			{
				if (i >= max_iter)
					break;

				int x = Random.Range(0, cellGrid.Width);
				int y = Random.Range(0, cellGrid.Height);

				gridCell = cellGrid.Grid[x, y];
				targetNode = cellGrid.GetPathfindingNode(gridCell);

				i++;
			}
			
			// Pathfind to cell
			currentPath = PathFinding.FindPath(currentNode, targetNode);
		}

		return true;
	}
	
	protected bool AttackPlayer()
	{
		// FULL SPEED AHEAD
		targetMovementSpeed = 1.0f;
		
		// Look for player towards the last known position
		// Calculate intercept vector
		Vector3 dir = (playerPos - transform.position).normalized;
		
		// Find nearest cell with cover from player
		PathNode coverNode;
		Vector3 coverPos;
		if (currentPath == null)
		{
			if (FindCover(out coverNode, out coverPos))
			{
				// Get node position
				Vector3 nodePos = cellGrid.GetCellPos(
					cellGrid.Grid[(int)coverNode.Position.x, (int)coverNode.Position.y]);

				// Path find to cover (or just the farthest away node if we didn't find cover)
				currentPath = PathFinding.FindPath(currentNode, coverNode);

				// If the path length is 1 (we're already on the cell), move towards hidePos
				if (currentPath.Count == 1)
				{
					Move((coverPos - transform.position));
				}
			}
			// If we couldn't find any cover, just run away
			else
			{
				Move(-dir);
			}
		}
		
		// If we can see the player, shoot at it
		if (CanSeeObjectOnLayer(dir, playerLayer, "Player"))
		{
			dir = Quaternion.AngleAxis(Random.Range(-0.5f, 0.5f) * LOOK_PLAYER_JITTER, Vector3.forward) *
				(playerPos - transform.position).normalized;
			
			TurnTowards(dir);

			// Shoot once we're facing the right direction
			if (Vector3.Angle(dir, FacingDirection) < 1.0f)
				Fire();
		}
		// Otherwise, look for it
		else
		{
			SwitchState(State.LOOK_FOR_PLAYER);
			return false;
		}
		
		return true;
	}

	protected bool LookForPlayer()
	{
		const float MAX_TIME_LOOKING = 1.3f;

		// Time spent looking to the left and then to the right
		// The rest of the time is spent spinning around
		// Looking to right time should be 2 * looking to left time
		// since it has to undo the looking to left time
		const float LOOKING_TO_LEFT_TIME = 0.3f;
		const float LOOKING_TO_RIGHT_TIME = 0.6f;

		float dt = Time.time - stateStartTime;
		
		// Check if we can see the player and switch to chase player if so
		if (CanSeeObjectOnLayer(FacingDirection, playerLayer, "Player"))
		{
			SwitchState(State.ATTACK_PLAYER);
			currentPath = null;
			return false;
		}

		// Get player's last node
		PathNode lastPlayerNode = cellGrid.GetPathfindingNode(cellGrid.GetCellAtPos(playerPos));
		
		// Move to player's last known location if not there
		if (gotPlayerPos)
		{
			if (!currentNode.Equals(lastPlayerNode) && currentPath == null)
			{
				if (Time.time - lastTimePlayerPathCalculated > PLAYER_PATH_REGEN_TIME)
				{
					lastTimePlayerPathCalculated = Time.time;

					// Otherwise, pathfind to the last known position
					List<PathNode> newPath = PathFinding.FindPath(currentNode,
					                                              cellGrid.GetPathfindingNode(cellGrid.GetCellAtPos(playerPos)));
					
					// If we couldn't find a path there, look for the player at our current location
					if (newPath.Count != 0)
					{
						currentPath = newPath;
					}
					else
					{
						// If there's no way to get to the player from our current location,
						// Keep patroling the maze
						SwitchState(State.MAP_LEVEL);
						return false;
					}
				}
			}
		}

		if (currentNode.Equals(lastPlayerNode) || !gotPlayerPos)
		{
			// Only look for MAX_ITME_LOOKING seconds and then go back to normal activity
			if (dt > MAX_TIME_LOOKING)
			{
				SwitchState(State.MAP_LEVEL);
				currentPath = null;
				return false;
			}

			// Turn to the left to look for player
			if (dt < LOOKING_TO_LEFT_TIME)
				facingDirectionTarget = Quaternion.AngleAxis(-10.0f, Vector3.forward) * facingDirectionTarget;
			// Turn to the right to look for player
			else if (dt > LOOKING_TO_LEFT_TIME && dt < LOOKING_TO_LEFT_TIME + LOOKING_TO_RIGHT_TIME)
				facingDirectionTarget = Quaternion.AngleAxis(10.0f, Vector3.forward) * facingDirectionTarget;
			// Just spin around in circles
			else
				facingDirectionTarget = Quaternion.AngleAxis(-10.0f, Vector3.forward) * facingDirectionTarget;
		}
		else
		{
			TurnTowards(movementDir);
		}

		return true;
	}
	
	protected bool CanSeeObjectOnLayer(Vector2 dir, LayerMask layer, string tag)
	{
		Vector3 pos;
		return CanSeeObjectOnLayer(dir, layer, tag, out pos);
	}
		
	protected bool CanSeeObjectOnLayer(Vector2 dir, LayerMask layer, string tag, out Vector3 objectPosition)
	{
		// Raycast to walls
		Vector3 startDir = Quaternion.AngleAxis(-0.5f * FOV, Vector3.forward) * dir;
		Vector3 endDir = Quaternion.AngleAxis(0.5f * FOV, Vector3.forward) * dir;
		
		for (int i = 0; i < FOV_STEPS; ++i)
		{
			Vector3 direction = Vector3.Lerp(startDir, endDir, (float)i * (1.0f / FOV_STEPS)).normalized;
			
			#if NPC_DEBUG && DEBUG_PLAYER_RAYCASTS
			lineRenderer.SetVertexCount(i*3+3);
			lineRenderer.SetPosition(i*3 + 0, transform.position);
			lineRenderer.SetPosition(i*3 + 1, transform.position + direction * 1000.0f);
			lineRenderer.SetPosition(i*3 + 2, transform.position);
			#endif
			
			Ray ray = new Ray(transform.position, direction);
			
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 1000.0f, layer))
			{
				if (tag.Equals(hitInfo.collider.tag))
				{
					lastPlayerPos = playerPos;
					
					gotPlayerPos = true;
					playerPos = hitInfo.collider.transform.position;
					
					// Send message to all other NPCs
					foreach (NonplayerCharacter npc in (NonplayerCharacter[])GameObject.
					         FindObjectsOfType(typeof(NonplayerCharacter)))
					{
						npc.SawPlayer(playerPos);
					}

					objectPosition = hitInfo.collider.transform.position;
					
					return true;
				}
			}
		}

		objectPosition = Vector3.zero;

		return false;
	}

	// Listen for messages from other NPCs
	protected void SawPlayer(Vector3 pos)
	{
		if (currentNode == null)
			return;

		if (currentState == State.PATROL_LEVEL ||
		    currentState == State.MAP_LEVEL)
		{
			playerPos = pos;
			gotPlayerPos = true;

			// Try and generate a path to the player
			List<PathNode> newPath = PathFinding.FindPath(currentNode,
			                                              cellGrid.GetPathfindingNode(cellGrid.GetCellAtPos(playerPos)));

			// If we succeeded in generating a path & we aren't currently chasing the player
			// Switch to going to find the player
			if (newPath.Count != 0)
			{
				currentPath = newPath;
				SwitchState(State.LOOK_FOR_PLAYER);
			}
		}
	}
	
	protected void TestVisibility()
	{
		// Raycast to walls
		Vector3 startDir = Quaternion.AngleAxis(-0.5f * FOV, Vector3.forward) * FacingDirection;
		Vector3 endDir = Quaternion.AngleAxis(0.5f * FOV, Vector3.forward) * FacingDirection;
		
		for (int i = 0; i < FOV_STEPS; ++i)
		{
			Vector3 direction = Vector3.Lerp(startDir, endDir, (float)i * (1.0f / FOV_STEPS)).normalized;
			Ray ray = new Ray(transform.position, direction);
			
#if NPC_DEBUG && DEBUG_FOV_RAYCASTS
			lineRenderer.SetVertexCount(i*3+3);
			lineRenderer.SetPosition(i*3 + 0, transform.position);
			lineRenderer.SetPosition(i*3 + 1, transform.position + direction * 1000.0f);
			lineRenderer.SetPosition(i*3 + 2, transform.position);
#endif
			
			int width = cellStates.GetLength(0);
			int height = cellStates.GetLength(1);
			
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 1000.0f, levelLayer))
			{
				if (LevelGen.CellGrid.IsInGrid(hitInfo.point))
				{
					Vector3 hitCellWorld = hitInfo.point - hitInfo.normal * 0.5f;
					
					Point hitCell = LevelGen.CellGrid.GetIdxFromPos(hitCellWorld);
					
					if (hitCell.x < 0 || hitCell.y < 0 ||
					    hitCell.x >= width || hitCell.y >= height)
					{
						continue;
					}

					int hitX = hitCell.x;
					int hitY = hitCell.y;
					
					cellStates[hitX, hitY] = CellState.BLOCKED;
					pathFindingGrid[hitX, hitY].Accessible = false;
					
					// Cast a ray in that direction through the grid from curX,curY to hitX,hitY
					// and mark the cells we pass through as clear
					Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
					
					Vector2 curPos = startPos;
					
					Vector2 direction2D = new Vector2(direction.x, direction.y);
					Vector2 invDirection2D = new Vector2(1.0f / direction.x, 1.0f / direction.y);
					
					float rayDirX = 5.0f * Mathf.Sign(direction2D.x);
					float rayDirY = 5.0f * Mathf.Sign(direction2D.y);
					
					Vector2 rayDir = new Vector2(rayDirX, rayDirY);
					
					int steps = (int)Mathf.Ceil(hitInfo.distance);
					
					for (int j = 0; j <= steps; ++j)
					{
						// TODO: fudged numbers
						// Make this more mathematically precise
						Vector3 pos = curPos + direction2D * ((float)j - 0.5f);
						
						Point posCell = LevelGen.CellGrid.GetIdxFromPos(pos);

						if (posCell.x >= 0 && posCell.y >= 0 &&
						    posCell.x < width && posCell.y < height)
						{
							int posX = posCell.x;
							int posY = posCell.y;
							
							if (posX != hitX && posY != hitY)
							{
								cellStates[posX, posY] = CellState.CLEAR;
								pathFindingGrid[posX, posY].Accessible = true;
							}
						}
					}
				}
			}
		}
	}
	
	protected void PathFind(int x, int y)
	{		
		// Pathfind to player
		CellGrid cellGrid = LevelGen.CellGrid;
		
		GridCell targetCell = cellGrid.Grid[x, y];
		GridCell currentCell = cellGrid.GetCellAtPos(transform.position);
		
		List<Vector3> path = cellGrid.FindPathWorld(targetCell, currentCell);
		
		if (path.Count > 1)
		{
			// Move towards next cell
			Vector3 dir = (path[1] - transform.position).normalized;
			TurnTowards(dir);
			Move(dir);
		}
	}
	
	protected List<PathNode> FindNearestPathToUnknown(PathNode start)
	{
		List<PathNode> open = new List<PathNode>();
		List<PathNode> closed = new List<PathNode>();
		
		// Add starting cell to open list
		start.Parent = null;
		start.GScore = 0;
		start.OverallScore = 0;
		open.Add(start);
		
		// Set end parent to null so we know afterwards if we found it
		PathNode end = null;
		float endScore = 99999;
		
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
			
			// Check adjacent nodes
			foreach (PathNode node in parent.AdjacentNodesNoDiag)
			{
				// Store parrent if node is unknown
				CellState state = cellStates[(int)node.Position.x, (int)node.Position.y];
				if (state == CellState.UNKNOWN)
				{
					float score = parent.GScore;
					
					if (currentTargets.Contains(node))
						score *= 4.0f;
					
					if (parent.GScore < endScore)
					{
						end = parent;
						endScore = parent.GScore;

						// End search if in fast mode (this will give us an
						// unknown, but not necessarily the closest one)
#if FAST_NODE
						goto end_search;
#endif
					}
				}
				
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
				
				// Calculate score
				float F = G;
				
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

#if FAST_NODE
	end_search:
#endif
		
		// Return an empty path if we couldn't find the end
		if (end == null)
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

	protected bool FindCover(out PathNode outNode, out Vector3 outPos)
	{
		const float STEP_OUT_DIST = 0.1f;

		List<PathNode> open = new List<PathNode>();
		List<PathNode> closed = new List<PathNode>();

		open.Add(currentNode);

		// Start at node and go outwards looking for a cell that provides cover
		while (open.Count > 0)
		{
			// Get next node and move it to the closed list
			PathNode next = open.First();
			open.Remove(next);
			closed.Add(next);

			// Check if this node provides cover
			Vector3 nodePos = cellGrid.GetCellPos(
				cellGrid.Grid[(int)next.Position.x, (int)next.Position.y]);
			nodePos += Vector3.back;

			// Get extremities in player direction
			Vector3 playerDir = (transform.position - playerPos).normalized;
			Vector3 perpendicularDir = Quaternion.AngleAxis(90, Vector3.forward) * playerDir;

			Vector3[] extremities = new Vector3[2];
			extremities[0] = nodePos + perpendicularDir * ((CapsuleCollider)collider).radius;
			extremities[1] = nodePos - perpendicularDir * ((CapsuleCollider)collider).radius;

			foreach (Vector3 extremity in extremities)
			{
				Vector3 diff = (extremity - playerPos);
				float dist = diff.magnitude;
				Vector3 dir = diff / dist;

#if NPC_DEBUG && DEBUG_COVER_RAYCASTS
				lineVertices += 3;
				lineRenderer.SetVertexCount(lineVertices);
				
				lineRenderer.SetPosition(lineVertices-3, playerPos);
				lineRenderer.SetPosition(lineVertices-2, extremity);
				lineRenderer.SetPosition(lineVertices-1, playerPos);
#endif

				// Raycast from player to extremity and see if we hit a wall
				RaycastHit hitInfo;
				if (Physics.Raycast(playerPos, dir, out hitInfo, dist, levelLayer))
				{
					Vector3 nodeSize = LevelGen.Instance.transform.lossyScale;
					nodeSize.z = 0;
					
					// Find node corners
					Vector3[] nodeCorners =
					{
						nodePos + new Vector3( nodeSize.x, 0, 0),
						nodePos + new Vector3(-nodeSize.x, 0, 0),
						nodePos + new Vector3(0,  nodeSize.y, 0),
						nodePos + new Vector3(0, -nodeSize.y, 0)
					};
					
					// Get nearest corner to player
					float closestSqrDist = (playerPos - nodeCorners[0]).sqrMagnitude;
					Vector3 closestCorner = nodeCorners[0];
					foreach (Vector3 corner in nodeCorners)
					{
						float sqrDist = (playerPos - corner).magnitude;
						
						if (sqrDist < closestSqrDist)
						{
							closestSqrDist = sqrDist;
							closestCorner = corner;
						}
					}
					
					Vector3 hidePos = nodePos;
					PathNode hideNode = null;
					
					for (int i = 0; i < 4; ++i)
					{
						// Hide behind the closest corner
						Vector3 hideDir = (closestCorner - playerPos).normalized;
						hidePos = closestCorner + hideDir;

						// Slide out until we can shoot the player
						Vector3 perpendicular = Quaternion.AngleAxis(90, Vector3.forward) * hideDir;

						float[] directions = { -1.0f, 1.0f };
						foreach (float direction in directions)
						{
							Vector3 steppedOutPos = hidePos + perpendicular * direction * STEP_OUT_DIST;
							float steppedOutDist = (playerPos - steppedOutPos).magnitude;
							Vector3 steppedOutDir = (playerPos - steppedOutPos) / steppedOutDist;
							
							Debug.DrawLine(playerPos, steppedOutPos);

							if (!Physics.Raycast(playerPos, (steppedOutPos - playerPos).normalized, steppedOutDist))
							{
								hidePos = steppedOutPos;
								break;
							}
						}
						
						// Path find to the node this is on (this might be different to the node we started with)
						GridCell hideCell = cellGrid.GetCellAtPos(hidePos);
						hideNode = cellGrid.GetPathfindingNode(hideCell);
						
						if (hideNode.Accessible)
						{
							outNode = next;
							outPos = hidePos;

							return true;
						}
					}
				}
			}

			// Add adjacent nodes to open list
			foreach (PathNode node in next.AdjacentNodesNoDiag)
			{
				if (node.Accessible && !open.Contains(node) && !closed.Contains(node))
					open.Add(node);
			}
		}

		outNode = currentNode;
		outPos = cellGrid.GetCellPos(
			cellGrid.Grid[(int)currentNode.Position.x, (int)currentNode.Position.y]);

		return false;
	}
}
