using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // The prefab for all the asteroids.
	public Asteroid asteroid;
    // The prefab for the player.
	public Player player;
    // The prefab for the UFO's, both big and small.
	public Ufo ufo;
    // The particle system prefab we spawn when something explodes.
    public ParticleSystem explodeParticles;

    // The number of asteroids to spawn for level 1.
	public int initialNumberOfAsteroids;
    // The amount to increase by per level.
	public int increasePerLevel;

    // A flag indicating whether the game is paused or not.
	public bool paused;

    // A flag indicating whether the drawObjcts should draw with white or with
    // their specified colors.
	public bool blackAndWhite;

    // The sound effect used in the game. A lot of these are used in multiple
    // classes so I just threw them here. All classes reference the manager anyway.
	public AudioClip musicBeat1;
	public AudioClip musicBeat2;
	public AudioClip explosionSmallClip;
	public AudioClip explosionMediumClip;
	public AudioClip explosionBigClip;
    public AudioClip bigUfoClip;
    public AudioClip smallUfoClip;
    public AudioClip extraLifeClip;
    public AudioClip thrustClip;
    public AudioClip shootClip;
    public AudioClip shieldHitClip;

    // Health mode. 1 is lives, 2 is recharging shields.
    public int healthMode;

    // Whether or not asteroids can collide.
    public bool asteroidCollision;

    // The score needed to receive an extra life. When this score is reached, the
    // score needed increases by the same amount.
    public int scoreForExtraLife;

    // The time in seconds between each UFO spawn.
    public float ufoSpawnTime;

    // Will get set to 1 when we initialize the first level.
    // Asteroids use this to determine their initial velocity.
    [HideInInspector]
    public int currentLevel = 0;

    // Local variables and references.
	Vector2[][] numbers;
	int score;
	int currentNumberOfAsteroids;
	int currentNumberToInitialize;
	GameObject[] children;
	int lives;
	GameObject livesGameObject;
	int activeProjectilesAtOnce;
	float musicTimer;
	float playBeat;
	float tempoTimer;
	float tempoReduceTime;
	float ufoTimer;
	
	StarField starfield;
	GameObject customSettingsToggle;
	GameObject classicDescription;
	GameObject customDescription;
	Toggle healthSettingLives;
	Toggle bulletSetting;
	Toggle collisionSetting;
	Toggle starfieldSetting;
	Transform scoreTransform;
	Transform highscoreTransform;
	GameObject settingsMenu;
	GameObject highscoreMenu;
	GameObject restartMenu;
    ServerScore serverScore;
	Button submitScoreButton;
	Text submitScoreText;
	Player playerInstance;
	bool playMusic;
	bool canSubmitScore;

	void Awake() {
		starfield = GameObject.Find("Starfield").GetComponent<StarField>();
		customSettingsToggle = GameObject.Find("CustomSettingsToggle");
        serverScore = GameObject.Find("ServerScore").GetComponent<ServerScore>();
		classicDescription = GameObject.Find("ClassicDescription");
		customDescription = GameObject.Find("CustomDescription");
		healthSettingLives = GameObject.Find("ToggleLives").GetComponent<Toggle>();
		bulletSetting = GameObject.Find("BulletSetting").GetComponent<Toggle>();
		collisionSetting = GameObject.Find("CollisionSetting").GetComponent<Toggle>();
		starfieldSetting = GameObject.Find("StarfieldSetting").GetComponent<Toggle>();
        submitScoreButton = GameObject.Find("SubmitScoreButton").GetComponent<Button>();
        submitScoreText = GameObject.Find("SubmitScoreText").GetComponent<Text>();
        settingsMenu = GameObject.Find("Settings");
        highscoreMenu = GameObject.Find("Highscore");
        restartMenu = GameObject.Find("RestartGame");
	}

	void Start() {
        // Create the game object which will hold the current score and the high score.
        GameObject go1 = new GameObject();
        go1.transform.parent = transform;
        scoreTransform = go1.transform;
        GameObject go2 = new GameObject();
        go2.transform.parent = transform;
        highscoreTransform = go2.transform;
        
        // Initialize the different menus and UI elements.
        highscoreMenu.SetActive(false);
        restartMenu.SetActive(false);
        classicDescription.SetActive(true);
        customDescription.SetActive(false);

        // Set the number of active projectiles at once.
        activeProjectilesAtOnce = player.activeProjectilesAtOnce;

        // Set the values for the music beat.
        playBeat = 1;
        tempoReduceTime = 1;
        playMusic = false;

        // This flag is set to false if playing in the custom mode.
        canSubmitScore = true;

		// The numbers 0-9 as points. The key is the ASCII code for the number
		// Why? Thought it would be fun to display the score with line renderers.
		numbers = new Vector2[64][];
		numbers[48] = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(0, -1), new Vector2(0, 1) };
		numbers[49] = new Vector2[] { new Vector2(1, 1), new Vector2(1, -1) };
		numbers[50] = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, -1) };
		numbers[51] = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1) };
		numbers[52] = new Vector2[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, -1) };
		numbers[53] = new Vector2[] { new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1) };
		numbers[54] = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, -1), new Vector2(1, 0), new Vector2(0, 0) };
		numbers[55] = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, -1) };
		numbers[56] = new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, -1), new Vector2(1, 0), new Vector2(0, 0) };
		numbers[57] = new Vector2[] {  new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, -1) };
	}

	void Update() {
		if (paused) {
            return;
        }

        // Increase the timers used.
		musicTimer += Time.deltaTime;
		tempoTimer += Time.deltaTime;
		ufoTimer += Time.deltaTime;

        // Play the music beat clips.
		if (playMusic && musicTimer > playBeat) {
			GetComponent<AudioSource>().clip = (GetComponent<AudioSource>().clip == musicBeat1) ? musicBeat2 : musicBeat1;
			GetComponent<AudioSource>().Play();
			musicTimer = 0;
		}

        // Increase the tempo of the music beat.
		if (tempoTimer > tempoReduceTime && playBeat >= 0.25f) {
			playBeat -= 0.01f;
			tempoTimer = 0;
		}

        // Spawn UFO's at regular intervals.
		if (ufoTimer > ufoSpawnTime) {
			Ufo ufoInstance = Instantiate(ufo) as Ufo;
			ufoInstance.healthMode = healthMode;
            // 40% of the UFO's will be small.
			if (Random.value > 0.1f) {
				ufoInstance.ufoType = 2;
				ufoInstance.shieldStrength = 2;
				ufoInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			}
			ufoTimer = 0;
		}
	}

    /**
     * Set the game preset (mode). I think I changed the name of this half way
     * through so it's a bit confusing. Called by the game mode radio buttons
     * in the main menu.
     */
	public void SetPreset(int value) {
		// Classic preset.
		if (value == 1) {
			classicDescription.SetActive(true);
			customDescription.SetActive(false);
			healthSettingLives.isOn = true;
			bulletSetting.isOn = true;
			collisionSetting.isOn = false;
			starfieldSetting.isOn = false;
			ToggleCustomSettings(false);
			canSubmitScore = true;
		}
		// Custom. Let the user choose his own settings.
		else if (value == 2) {
			classicDescription.SetActive(false);
			customDescription.SetActive(true);
			ToggleCustomSettings(true);
			canSubmitScore = false;
		}
	}

    /**
     * Hides/shows an image that lies on top of the custom settings fields in the
     * menu. When it's shown the settings looks like they're disabled and any 
     * interaction won't pass through. When it's hidden you can interact with 
     * the controls as normal.
     */
	void ToggleCustomSettings(bool value) {
		customSettingsToggle.SetActive(!value);
	}

    /**
     * Enables/disable asteroid collision.
     */
	public void SetCollisionMode(bool value) {
		Physics2D.IgnoreLayerCollision(10, 10, !value);
		asteroidCollision = value;
	}

    /**
     * Sets the health mode.
     */
	public void SetHealthMode(int value) {
		healthMode = value;
	}

    /**
     * Sets the mode determining whether the player should be able to fire more
     * than 4 bullets at once or not.
     */
	public void SetBulletMode(bool value) {
		if (value) {
			activeProjectilesAtOnce = 4;
		}
		else {
			activeProjectilesAtOnce = 0;
		}
	}

    /**
     * Toggles the star field on/off.
     */
	public void SetStarfieldOnOff(bool value) {
		starfield.Toggle(value);
	}

    /**
     * Sets the volume of the audio listener and in term the game master volume.
     */
	public void SetVolume(float value) {
		AudioListener.volume = value / 100;
		Text volumeText = GameObject.Find("VolumeSettingValue").GetComponent<Text>();
		volumeText.text = value.ToString();
	}

    /**
     * Starts the game. Called by the start button in the menu.
     */
	public void StartGame() {
		// Hide the menu and unpause the game.
		settingsMenu.SetActive(false);
		paused = false;

        // Draw the player's lives according to the health mode.
        // The game manager draws shield strength as lives as well so it doesn't
        // really care. It just uses a different graphic.
		if (healthMode == 2) {
			lives = player.shieldStrength;
		}
		else {
			lives = player.lives;
		}
		livesGameObject = new GameObject();
		DrawLives();
		
        // Display the initial score. The score is only drawn when it changes.
		DisplayScore();

        // Set the initial number of asteroids to spawn. We subtract the increase
        // per level because it gets added back again in the InitializeLevel()
        // method.
        currentNumberToInitialize = initialNumberOfAsteroids - increasePerLevel;

        // Initialize the first level.
		StartCoroutine(InitializeLevel());
	}

    /**
     * Draw the player's lives in the top left corner.
     */
	void DrawLives() {
        // Remove the old lives.
		foreach (Transform child in livesGameObject.transform) {
			Destroy(child.gameObject);
		}

        // Create each life as a separate game object with a line renderer.
        // It's the DrawObject class which does the actual drawing.
		for (int i = 0; i < lives; i++) {
			GameObject go = new GameObject();
			go.transform.parent = livesGameObject.transform;
			go.transform.position = transform.position + new Vector3(-460, 260, 0);
			int offset = (healthMode == 1) ? 16 : 4;
			go.transform.localPosition = new Vector3(go.transform.localPosition.x + (offset * i), go.transform.localPosition.y, go.transform.localPosition.z);
			float scale = (healthMode == 1) ? 0.5f : 1f;
			go.transform.localScale = transform.localScale * scale;
			
            go.AddComponent<DrawObject>();
			DrawObject drawObject = go.GetComponent<DrawObject>();
			drawObject.color = Color.green;
			drawObject.width = 1;
			List<Vector3> drawPoints = new List<Vector3>();
            // If the health mode is set to lives this draws small player ships. 
			if (healthMode == 1) {
				for (int j = 0; j < player.shipPoints.Length; j++) {
					drawPoints.Add(new Vector3(player.shipPoints[j].x, player.shipPoints[j].y, 0));
				}
			}
            // Otherwise it draws vertical lines.
			else {
				drawPoints.Add(new Vector3(0, 8, 0));
				drawPoints.Add(new Vector3(0, -8, 0));
			}
			drawObject.SetPoints(drawPoints);
		}
	}

	/**
	 * Initialize the current level.
	 * 
	 * Spawns all the asteroids and resets the music beat.
	 */
	IEnumerator InitializeLevel() {
		playMusic = false;

		currentLevel++;

		yield return new WaitForSeconds(1);

		if (!playerInstance) {
			playerInstance = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity) as Player;
			playerInstance.activeProjectilesAtOnce = activeProjectilesAtOnce;
			playerInstance.healthMode = healthMode;
		}
		
		currentNumberToInitialize += increasePerLevel;
		
		// Get the current position of the player. We use this to make sure we don't spawn asteroids on him.
		Vector2 currentPlayerPosition = new Vector2(playerInstance.transform.position.x, playerInstance.transform.position.y);
		
		for (int i = 0; i < currentNumberToInitialize; i++) {
			// Find a position which isn't too close to the player.
			Vector2 newPosition = currentPlayerPosition;
			while (Vector2.Distance(newPosition, currentPlayerPosition) < 200) {
				newPosition = new Vector2(Random.Range(-Screen.width / 2, Screen.width / 2), Random.Range(-Screen.height / 2, Screen.height / 2));
			}
			
			// Spawn an ansteroid at this position.
			Instantiate(asteroid, new Vector3(newPosition.x, newPosition.y, 0), transform.rotation);
			AddAsteroid();
		}
		
		// Reset the beat.
		playMusic = true;
		playBeat = 1f;
	}
    
    /**
     * Add one life and then draw the player's lives again.
     */
	public void AddLife() {
		lives++;
		DrawLives();
	}

    /**
     * Remove one of the player's lives and then draw the lives again.
     */
	public void RemoveLife(int value) {
		lives -= value;
		DrawLives();
	}
	
    /**
     * Called when an asteroid is spawned.
     */
	public void AddAsteroid() {
		currentNumberOfAsteroids++;
	}
	
    /**
     * Called when an asteroid is destroyed.
     */
	public void RemoveAsteroid() {
		currentNumberOfAsteroids--;
		if (currentNumberOfAsteroids <= 0) {
			StartCoroutine(InitializeLevel());
		}
	}
	
    /**
     * Add to the player's score and update it.
     */
	public void AddScore(int value) {
		score += value;

		if (healthMode == 1 && score >= scoreForExtraLife) {
			scoreForExtraLife += scoreForExtraLife;
			StartCoroutine(GiveExtraLife());
		}

		DisplayScore();
	}

    /**
     * Gives the player an extra life and plays a sound effect.
     */
	IEnumerator GiveExtraLife() { 
		AddLife();

		// Play the extra sound audio clip 10 times in rapid succession.
		for (int i = 0; i < 10; i++) {
			GetComponent<AudioSource>().PlayOneShot(extraLifeClip);
			yield return new WaitForSeconds(extraLifeClip.length / 1.5f);
		}
	}
	
    /**
     * Displays the current player's score in the top of the screen using
     * line renderers.
     */
	void DisplayScore() {
		string text = score.ToString();

        // Remove the old score.
		foreach (Transform child in scoreTransform) {
			Destroy(child.gameObject);
		}

        // Create each character as a separate game object with a line renderer.
        // It's the DrawObject class which does the actual drawing.
		for (int i = 0; i < text.Length; i++) {
			int charNum = System.Convert.ToInt32(text[i]);
			
			GameObject go = new GameObject();
			go.transform.parent = scoreTransform;
			go.transform.position = transform.position + new Vector3(0 - (25 * text.Length / 2), 250, 0);
			go.transform.localPosition = new Vector3(go.transform.localPosition.x + (25 * i), go.transform.localPosition.y, go.transform.localPosition.z);
			go.transform.localScale = transform.localScale * 16;
			go.AddComponent<DrawObject>();
			
            DrawObject drawObject = go.GetComponent<DrawObject>();
			drawObject.color = Color.blue;
			drawObject.width = 2;
			List<Vector3> drawPoints = new List<Vector3>();
			for (int j = 0; j < numbers[charNum].Length; j++) {
				drawPoints.Add(new Vector3(numbers[charNum][j].x, numbers[charNum][j].y, 0));
			}
			drawObject.SetPoints(drawPoints);
		}
	}

    /**
     * Displays the high score in the top right corner of the screen using
     * line renderers.
     */
	public void DisplayHighscore(int highscore) {
		string text = highscore.ToString();

        // Reverse the text so we can display it in the top right corner properly
        // aligned regardless of the number of characters.
		text = Reverse(text);

        // Remove the old high score.
		foreach (Transform child in highscoreTransform) {
			Destroy(child.gameObject);
		}
		
        // Create each character as a separate game object with a line renderer.
        // It's the DrawObject class which does the actual drawing.
		for (int i = 0; i < text.Length; i++) {
			int charNum = System.Convert.ToInt32(text[i]);
			
			GameObject go = new GameObject();
			go.transform.parent = highscoreTransform;
			go.transform.position = transform.position + new Vector3(440, 260, 0);
			go.transform.localPosition = new Vector3(go.transform.localPosition.x - (12 * i), go.transform.localPosition.y, go.transform.localPosition.z);
			go.transform.localScale = transform.localScale * 8;
			
            go.AddComponent<DrawObject>();
			DrawObject drawObject = go.GetComponent<DrawObject>();
			drawObject.color = Color.blue;
			drawObject.width = 2; 
			List<Vector3> drawPoints = new List<Vector3>();
			for (int j = 0; j < numbers[charNum].Length; j++) {
				drawPoints.Add(new Vector3(numbers[charNum][j].x, numbers[charNum][j].y, 0));
			}
			drawObject.SetPoints(drawPoints);
		}
	}

    /**
     * The game is over. The menu we display depends on whether or not we're
     * allowed to submit our score or not.
     */
	public void GameOver() {
		paused = true;
		if (canSubmitScore) {
			highscoreMenu.SetActive(true);
		}
		else {
			restartMenu.SetActive(true);
		}
	}

    /**
     * Called by the submit score button in the menu.
     */
	public void SubmitScore() {
		string playerName = GameObject.Find("PlayerName").GetComponent<Text>().text;
		if (playerName.Length < 1) {
			submitScoreText.text = "You must enter a name";
		}
		else {
			submitScoreText.text = "Uploading...";
			submitScoreButton.interactable = false;
            StartCoroutine(serverScore.UploadScore(playerName, score));
		}
	}

    /**
     * Hides the score menu and shows the restart menu if the score was
     * successfully uploaded.
     */
	public void ScoreUploaded() {
		highscoreMenu.SetActive(false);
		restartMenu.SetActive(true);
	}

    /**
     * Restart the game. Called by the restart game button in the menu.
     */
	public void RestartGame() {
        SceneManager.LoadScene(0);
	}

    /**
     * Helper method for reversing a string.
     */
	string Reverse(string text) {
		char[] array = text.ToCharArray();
		System.Array.Reverse(array);
		return new System.String(array);
	}
}
