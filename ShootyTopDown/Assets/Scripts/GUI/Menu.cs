//#define NPC_DEBUG

using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
	public enum MenuState
	{
		TITLE,
		GAMEMODESETUP,
		GAME,
		FADETOPAUSE,
		PAUSE,
		FADETOGAME
	}

	private const float FADE_SPEED = 5.0f;
	private const float FADE_MAX = 0.7f;

	private const float LIGHT_CHANCE = 0.1f;

	private const float DEFAULT_WIDTH = 1680;

	private const float ROUND_START_DELAY = 4.0f;

	private const int MIN_MAZE_SIZE = 9;
	private const int MAX_MAZE_SIZE = 71;

	private const int MIN_ENEMY_COUNT = 0;
	private const int MAX_ENEMY_COUNT = 50;

	public MenuState menuState = MenuState.TITLE;

	public GameObject playerCharacterPrefab;
	public GameObject playerCameraPrefab;
	public GameObject nonPlayerCharacterPrefab;

	public Texture2D titleTexture;
	public Texture2D clickToBeginTexture;
	public Texture2D settingsTitleTexture;

	public Texture healthBarBack;
	public Texture healthBarFront;
	public Texture healthBarBorder;
	
	private GameObject levelObj;
	private GameObject playerCharacterObj;
	private GameObject playerCameraObj;
	
	private PlayerCharacter playerCharacter;
	
	private Maze maze;
	private LevelGen levelGen;
	
	private float enemyCount = 5;
	private float mazeDimensions = 15;
	private bool generateLoops = true;

	private bool clicked = false;
	private bool escPressed = false;

	private float fadeVal = 0.0f;
	private Texture2D fadeTexture;
	private float fadeStartTime;

	private float roundStartTime;

#if NPC_DEBUG
	private Texture2D tex;
	private Texture2D tex2;
#endif

	protected void Start()
	{
		// Get level object
		levelObj = GameObject.Find("Level");
		maze = levelObj.GetComponent<Maze>();
		levelGen = levelObj.GetComponent<LevelGen>();

		// Create fade texture (used for pause screen fade)
		fadeTexture = new Texture2D(1, 1);
	}
	
	protected void StartGame()
	{
		// End game if not ended
		EndGame();

		// Set round start timer
		roundStartTime = Time.realtimeSinceStartup;

		// Create debug textures for npc maps
#if NPC_DEBUG
		tex = new Texture2D(maze.width, maze.height);
		tex.filterMode = FilterMode.Point;
		tex2 = new Texture2D(maze.width, maze.height);
		tex2.filterMode = FilterMode.Point;
#endif
		
		// Create player character
		playerCharacterObj = (GameObject)GameObject.Instantiate(playerCharacterPrefab);
		playerCameraObj = (GameObject)GameObject.Instantiate(playerCameraPrefab);

		// Get player character component
		playerCharacter = playerCharacterObj.GetComponent<PlayerCharacter>();
		playerCharacter.Health = CharacterBase.MaxHealth;

		// Generate level
		levelGen.Regenerate();

		// Move player camera over player
		Vector3 newCamPos = playerCharacterObj.transform.position;
		newCamPos.z = playerCameraObj.transform.position.z;
		playerCameraObj.transform.position = newCamPos;
	}
	
	protected void EndGame()
	{
#if NPC_DEBUG
		if (tex != null)
			Destroy(tex);
		if (tex2 != null)
			Destroy(tex2);
#endif

		// Destroy stuff
		DestroyIfNotNull(playerCharacterObj);
		DestroyIfNotNull(playerCameraObj);
		
		// Clean up projectiles
		foreach (Component projectile in GameObject.FindObjectsOfType(typeof(Projectile)))
		{
			Destroy(projectile.gameObject);
		}

		// Ungenerate level
		levelGen.Ungenerate();
	}

	protected void Update()
	{
		bool justClicked = false;
		bool escJustPressed = false;

		if (Input.GetMouseButton(0) && !clicked)
		{
			justClicked = true;
			clicked = true;
		}
		
		if (Input.GetKey(KeyCode.Escape) && !escPressed)
		{
			escJustPressed = true;
			escPressed = true;
		}

		switch (menuState)
		{
		case MenuState.TITLE:
			if (justClicked)
			{
				menuState = MenuState.GAMEMODESETUP;
			}
			else if (escJustPressed)
			{
				Application.Quit();
			}
			break;
		case MenuState.GAMEMODESETUP:
			if (escJustPressed)
			{
				menuState = MenuState.TITLE;
			}
			break;
		case MenuState.GAME:

			if (Time.realtimeSinceStartup - roundStartTime < ROUND_START_DELAY)
			{
				Time.timeScale = 0.0f;
			}
			else
			{
				Time.timeScale = 1.0f;

				if (escJustPressed)
				{
					fadeStartTime = Time.realtimeSinceStartup;
					menuState = MenuState.FADETOPAUSE;
				}
				
				if (playerCharacter.Health <= 0.0f || GameObject.FindObjectsOfType(typeof(NonplayerCharacter)).Length == 0)
				{
					fadeStartTime = Time.realtimeSinceStartup;
					menuState = MenuState.FADETOPAUSE;
				}
			}

			break;
		case MenuState.PAUSE:
			Time.timeScale = 0.0f;
			
			if (escJustPressed && !playerCharacter.Dead && GameObject.FindObjectsOfType(typeof(NonplayerCharacter)).Length > 0)
			{
				fadeStartTime = Time.realtimeSinceStartup;
				menuState = MenuState.FADETOGAME;
			}

			break;
		case MenuState.FADETOPAUSE:
			Time.timeScale = 0.0f;

			fadeVal = Mathf.Lerp(0.0f, FADE_MAX, (Time.realtimeSinceStartup - fadeStartTime) * FADE_SPEED);
			
			if (fadeVal == FADE_MAX)
				menuState = MenuState.PAUSE;
			
			break;
		case MenuState.FADETOGAME:
			Time.timeScale = 0.0f;

			fadeVal = Mathf.Lerp(FADE_MAX, 0.0f, (Time.realtimeSinceStartup - fadeStartTime) * FADE_SPEED);
			
			if (fadeVal == 0.0f)
				menuState = MenuState.GAME;
			
			break;
		}

		if (clicked && !Input.GetMouseButton(0))
			clicked = false;
		if (escPressed && !Input.GetKey(KeyCode.Escape))
			escPressed = false;

#if NPC_DEBUG
		if (tex != null)
		{
			// Update grid texture
			for (int x = 0; x < maze.width; ++x)
				for (int y = 0; y < maze.height; ++y)
			{
				Color colour = Color.black;
				
				switch (NonplayerCharacter.CellStates[x, y])
				{
				case CellState.BLOCKED:
					colour = Color.red;
					break;
				case CellState.CLEAR:
					colour = Color.green;
					break;
				case CellState.UNKNOWN:
					colour = Color.blue;
					break;
				}
				
				tex.SetPixel(x, y, colour);
			}
			
			tex.Apply();
		}

		if (tex2 != null)
		{
			// Update pathfinding texture
			for (int x = 0; x < maze.width; ++x)
				for (int y = 0; y < maze.height; ++y)
			{
				Color colour = Color.black;
				
				if (NonplayerCharacter.PathFindingGrid[x, y].Accessible)
					colour = Color.white;
				
				tex2.SetPixel(x, y, colour);
			}
			
			// Update texture
			tex2.Apply();
		}
#endif
	}
	
	protected void OnGUI()
	{
		float guiScale = Screen.width / DEFAULT_WIDTH;

		switch (menuState)
		{
		case MenuState.TITLE:
			DrawTitle(guiScale);
			break;
		case MenuState.GAMEMODESETUP:
			DrawGamemodeSetup(guiScale);
			break;
		case MenuState.GAME:
			DrawGameUI(guiScale);
			break;
		case MenuState.PAUSE:
			DrawFadeTexture();
			DrawPauseMenu(guiScale);
			break;
		case MenuState.FADETOGAME:
		case MenuState.FADETOPAUSE:
			DrawFadeTexture();
			break;
		}
	}

	protected void DrawTitle(float guiScale)
	{
		// Title screen
		// Draw title
		Rect titleRect  = new Rect((0.5f * Screen.width - 0.5f * (titleTexture.width * guiScale)),
		                           (0.35f * Screen.height - 0.5f * (titleTexture.height * guiScale)),
		                           titleTexture.width * guiScale, titleTexture.height * guiScale);
		
		GUI.DrawTexture(titleRect, titleTexture);
		
		// Draw "click to play" text
		Rect clickToPlayRect = new Rect((0.5f * Screen.width - 0.5f * (clickToBeginTexture.width * guiScale)),
		                                (0.8f * Screen.height - 0.5f * (clickToBeginTexture.height * guiScale)),
		                                clickToBeginTexture.width * guiScale, clickToBeginTexture.height * guiScale);
		
		if (Time.time % 2.0f < 1.4f)
			GUI.DrawTexture(clickToPlayRect, clickToBeginTexture);
	}
	
	protected void DrawGamemodeSetup(float guiScale)
	{
		const int width = 400;
		int height = Screen.height;

		// Draw settings title
		Rect settingsTitleRect = new Rect((0.5f * Screen.width - 0.5f * (settingsTitleTexture.width * 0.5f)),
		                                  (0.25f * Screen.height - 0.5f * (settingsTitleTexture.height * 0.5f)),
		                                  settingsTitleTexture.width * 0.5f, settingsTitleTexture.height * 0.5f);
		
		GUI.DrawTexture(settingsTitleRect, settingsTitleTexture);

		
		GUILayout.BeginArea(new Rect(0.5f * (Screen.width - width),
		                             0.5f * (Screen.height),
		                             width,
		                             height));
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Label("Map size:", new GUILayoutOption[] { GUILayout.Width(100) });
		
		// Show value
		GUILayout.Label(maze.width + "x" + maze.height, new GUILayoutOption[] { GUILayout.Width(50) });
		
		// Maze dimensions slider
		{
			mazeDimensions = GUILayout.HorizontalSlider(mazeDimensions, MIN_MAZE_SIZE, MAX_MAZE_SIZE);
			
			// Update values
			maze.width = (int)mazeDimensions;
			maze.height = (int)mazeDimensions;
			levelGen.lightCount = (int)(LIGHT_CHANCE * (maze.width * maze.height));
			
			// Round up to odd number
			if (maze.width % 2 == 0)
				maze.width += 1;
			if (maze.height % 2 == 0)
				maze.height += 1;
		}
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Label("Enemy count: ", new GUILayoutOption[] { GUILayout.Width(100) });
		
		// Show value
		GUILayout.Label(levelGen.enemyCount.ToString(), new GUILayoutOption[] { GUILayout.Width(50) });
		
		// Enemy count slider
		{
			enemyCount = GUILayout.HorizontalSlider(enemyCount, MIN_ENEMY_COUNT, MAX_ENEMY_COUNT);
			
			// Update values
			levelGen.enemyCount = (int)enemyCount;
		}
		
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();

		GUILayout.Label("Generate loops: ", new GUILayoutOption[] { GUILayout.Width(100) });

		generateLoops = GUILayout.Toggle(generateLoops, "");

		// Update value
		if (generateLoops)
			maze.generationMode = Maze.Mode.CREATELOOPS;
		else
			maze.generationMode = Maze.Mode.PERFECT;
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(25);
		
		if (GUILayout.Button("Play!"))
		{
			StartGame();
			menuState = MenuState.GAME;
		}
		
		GUILayout.EndArea();
	}
	
	protected void DrawPauseMenu(float guiScale)
	{
		const int width = 200;
		int height = Screen.height;
		
		GUILayout.BeginArea(new Rect(0.5f * (Screen.width - width),
		                             0.5f * (Screen.height),
		                             width,
		                             height));

		if (playerCharacter.Health > 0.0f &&
		    GameObject.FindObjectsOfType(typeof(NonplayerCharacter)).Length > 0 &&
		    GUILayout.Button("Return to game"))
		{
			fadeStartTime = Time.realtimeSinceStartup;
			menuState = MenuState.FADETOGAME;
		}

		if (GameObject.FindObjectsOfType(typeof(NonplayerCharacter)).Length == 0)
		{
			GUILayout.Label("You win!");
		}
		
		if (GUILayout.Button("Restart"))
		{
			StartGame();
			menuState = MenuState.GAME;
		}
		
		if (GUILayout.Button("Quit game"))
		{
			Time.timeScale = 1.0f;
			fadeVal = 0.0f;
			EndGame();
			menuState = MenuState.GAMEMODESETUP;
		}
		
		GUILayout.EndArea();
	}

	protected void DrawGameUI(float guiScale)
	{
		if (Time.realtimeSinceStartup - roundStartTime < ROUND_START_DELAY)
		{
			GUIStyle guiStyle = new GUIStyle();
			guiStyle.fontSize = 150;
			guiStyle.alignment = TextAnchor.MiddleCenter;

			GUILayout.Label(((int)(ROUND_START_DELAY - (Time.realtimeSinceStartup - roundStartTime))).ToString(), guiStyle,
			                new GUILayoutOption[] { GUILayout.Width(Screen.width), GUILayout.Height(Screen.height) });
		}
		else
		{
			// Draw healthbar
			const float HEALTH_BAR_BORDER_WIDTH = 1.0f;
			
			const float HEALTH_BAR_HEIGHT = 25;
			const float HEALTH_BAR_MAX_WIDTH = 100;
			
			float healthBarX = Screen.width / 2 - HEALTH_BAR_MAX_WIDTH / 2;
			float healthBarY = 10;
			
			float healthBarCoeff = playerCharacter.Health / CharacterBase.MaxHealth;
			float healthBarWidth = HEALTH_BAR_MAX_WIDTH * healthBarCoeff;
			
			GUI.DrawTexture(new Rect(healthBarX-HEALTH_BAR_BORDER_WIDTH,
			                         healthBarY-HEALTH_BAR_BORDER_WIDTH,
			                         healthBarWidth+HEALTH_BAR_BORDER_WIDTH*2,
			                         HEALTH_BAR_HEIGHT+HEALTH_BAR_BORDER_WIDTH*2), healthBarBorder);
			GUI.DrawTexture(new Rect(healthBarX,
			                         healthBarY,
			                         HEALTH_BAR_MAX_WIDTH,
			                         HEALTH_BAR_HEIGHT), healthBarBack);
			GUI.DrawTexture(new Rect(healthBarX,
			                         healthBarY,
			                         healthBarWidth,
			                         HEALTH_BAR_HEIGHT), healthBarFront);

			GUIStyle style = new GUIStyle();
			GUI.Label(new Rect(healthBarX + 5, healthBarY + 5, HEALTH_BAR_MAX_WIDTH, HEALTH_BAR_HEIGHT), ((int)playerCharacter.Health).ToString(), style);

			// Draw weapon and ammo type
			string weaponName = CharacterBase.WeaponTypeToName(playerCharacter.CurrentWeapon);
			string ammoCount = (playerCharacter.CurrentWeapon == PlayerCharacter.WeaponType.PISTOL ?
			                    "Infinity" : playerCharacter.CurrentAmmo.ToString());

			style.fontSize = 22;
			style.alignment = TextAnchor.LowerLeft;
			GUILayout.Label("  Current weapon: " + weaponName + "\n" +
			                "  Current ammo: " + ammoCount + "\n", style,
			                new GUILayoutOption[] { GUILayout.Width(Screen.width), GUILayout.Height(Screen.height) });
		}
		
#if NPC_DEBUG
		if (tex != null && tex2 != null)
		{
			Vector2 textureSize = new Vector2(150, 150);
			
			GUI.DrawTexture(new Rect(0, 0, textureSize.x, textureSize.y), tex);
			GUI.DrawTexture(new Rect(0, textureSize.y, textureSize.x, textureSize.y), tex2);
			GUI.Label(new Rect(textureSize.x + 5, 5, 100, 20), "FPS: " + (1.0f / Time.deltaTime));
		}
#endif
	}

	protected void DrawFadeTexture()
	{
		Color col = Color.black;
		col.a = fadeVal;

		fadeTexture.SetPixel(0, 0, col);
		fadeTexture.Apply();

		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture, ScaleMode.StretchToFill, true);
	}

	private void DestroyIfNotNull(UnityEngine.Object obj)
	{
		if (obj != null)
		{
			Destroy(obj);
		}
	}
}
