using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	// Our forward speed. Acceleration I guess.
    public float speed;
	// Our rotate speed.
    public float rotationSpeed;

	// The projectile we shoot.
    public Projectile projectile;
	// The speed of the projectiles we shoot.
    public float projectileSpeed;
	// The number of projectiles we can have in the air at once. The classic only allowed 4.
	public int activeProjectilesAtOnce;
	// The number of projectiles we can shoot per second.
	public float shotsPerSecond;

	// The points defining the shape of our ship.
	public Vector2[] shipPoints;

	// The number of lives we have if we're playing with lives.
	public int lives;
	// The number of shield points we have if we're playing with recharging shields.
	public int shieldStrength;
    // The duration we're invulnerable before we can take another hit.
	public float shieldInvulnDuration;
    // The time in seconds before we recharge 1 shield point.
    public float shieldRechangeDuration;

    // The time in seconds to keep the flame turned on and off.
    public float flameDuration;

    // The time in seconds we are invulnerable after respawning.
    public float deathInvulnDuration;

	// The current health mode. Public so the game manager can set it.
	[HideInInspector]
	public int healthMode;
	// The number of projectiles currently in the air. Public so the projectiles can subtract from it.
	[HideInInspector]
	public int activeProjectiles;

	// Local variables and references.
	int currentShieldStrength;
	float shieldInvulnTimer;
	float shieldRechargeTimer;
    float nextFire;
	DrawObject drawObject;
	PolygonCollider2D polyCollider;
	float turning;
	float thrust;
	bool disabled;
	List<Vector3> drawPoints;
	GameManager manager;
	Shield shield;
	GameObject flame;
	float flameInactiveTimer;
	float flameActiveTimer;
	float deathInvulnTimer;
	float fireRate;

	void Awake() {
		manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		shield = transform.Find("Shield").GetComponent<Shield>();
	}

    void Start() {
        disabled = false;
        shield.shieldInvulnDuration = shieldInvulnDuration;
        currentShieldStrength = shieldStrength;
        fireRate = 1 / shotsPerSecond;

        // Setup the draw object.
        drawObject = GetComponent<DrawObject>();
        drawPoints = new List<Vector3>();
        for (int i = 0; i < shipPoints.Length; i++) {
            Vector3 pos = new Vector3(shipPoints[i].x, shipPoints[i].y, 0);
			drawPoints.Add(pos);
        }
        drawObject.SetPoints(drawPoints);

        // Setup the polygon collider.
		polyCollider = GetComponent<PolygonCollider2D>();
		polyCollider.points = shipPoints;

		// Add the lines making up our flame.
		flame = new GameObject();
		flame.transform.parent = transform;
		flame.transform.position = transform.position;
		flame.AddComponent<DrawObject>();
		DrawObject flameDrawObject = flame.GetComponent<DrawObject>();
        flameDrawObject.color = drawObject.color;
        flameDrawObject.width = drawObject.width;
		List<Vector3> flameLinePoints = new List<Vector3>();
		flameLinePoints.Add(new Vector3(-4, -12, 0));
		flameLinePoints.Add(new Vector3(0, -24, 0));
		flameLinePoints.Add(new Vector3(4, -12, 0));
		flameDrawObject.SetPoints(flameLinePoints);
        flame.GetComponent<Renderer>().enabled = false;
    }

	void Update() {
		if (manager.paused) {
			return;
		}

		if (disabled) {
			return;
		}

        // Increment all our timers.
        if (healthMode == 2) {
            shieldInvulnTimer += Time.deltaTime;
            shieldRechargeTimer += Time.deltaTime;
        }
        nextFire += Time.deltaTime;
        deathInvulnTimer += Time.deltaTime;
        flameInactiveTimer += Time.deltaTime;

        // Recharging shields.
		if (healthMode == 2) {
			if (shieldRechargeTimer > shieldRechangeDuration && currentShieldStrength < shieldStrength) {
				currentShieldStrength++;
				manager.AddLife();
				shieldRechargeTimer = 0;
			}
		}

		if (deathInvulnTimer > deathInvulnDuration) {
			polyCollider.enabled = true;
		}

		// Input handling.
        if (Input.GetButton("Fire1") && nextFire > fireRate) {
            Shoot();
        }
		turning = Input.GetAxisRaw("Horizontal");
		thrust = Input.GetAxisRaw("Vertical");
		if (Input.GetButtonDown("Hyperspace")) {
			StartCoroutine("Hyperspace");
		}

        // Thrusting and flame handling.
		flame.GetComponent<Renderer>().enabled = false;
		if (thrust > 0) {
			if (flameInactiveTimer > flameDuration) {
				flame.GetComponent<Renderer>().enabled = true;
				flameActiveTimer += Time.deltaTime;
			}
			if (flameActiveTimer > flameDuration) {
				flameInactiveTimer = 0;
				flameActiveTimer = 0;
			}
            GetComponent<AudioSource>().clip = manager.thrustClip;
			if (!GetComponent<AudioSource>().isPlaying) {
				GetComponent<AudioSource>().Play();
			}
		}

		// Apply turning as a rotation.
		transform.Rotate(0, 0, turning * Time.deltaTime * rotationSpeed * -1);
	}

    /**
     * Shoot a single projectile.
     */
	void Shoot() {
		if (activeProjectilesAtOnce > 0 && activeProjectiles >= activeProjectilesAtOnce) {
			return;
		}

		Projectile instance = Instantiate(projectile, transform.position + transform.TransformDirection(0, 12, 0), transform.rotation) as Projectile;
		instance.GetComponent<Rigidbody2D>().velocity = transform.TransformDirection(0, projectileSpeed, 0) + new Vector3(GetComponent<Rigidbody2D>().velocity.x, GetComponent<Rigidbody2D>().velocity.y, 0);
		instance.player = this;
		instance.GetComponent<DrawObject>().color = drawObject.color;
		Physics2D.IgnoreCollision(GetComponent<Collider2D>(), instance.GetComponent<Collider2D>());
        AudioSource.PlayClipAtPoint(manager.shootClip, Camera.main.transform.position);
		nextFire = 0;
		activeProjectiles++;
	}

    void FixedUpdate() {
		if (manager.paused) {
			return;
		}

		// Apply thrust as a force.
		Vector3 force = transform.TransformDirection(0, thrust * speed, 0);
        GetComponent<Rigidbody2D>().AddForce(force);
    }

    /**
     * Teleports the player to a random spot. Taken directly from the original.
     */
	IEnumerator Hyperspace() {
        // Hide the player.
		HideAndDisablePlayer(true);

		// Wait a while.
		yield return new WaitForSeconds(0.5f);

        // Position ourself in a random spot.
		transform.position = new Vector3(Random.Range(-480f, 480f), Random.Range(-300f, 300f), 0);

        // Show the player.
		HideAndDisablePlayer(false);
	}

    /**
     * Called when we die.
     */
	IEnumerator Death() {
        // Hide the player.
		HideAndDisablePlayer(true);

		// Wait a while.
		yield return new WaitForSeconds(1f);

        // If we're out of lives or shield initiate game over.
		if (lives <= 0 || currentShieldStrength <= 0) {
			manager.GameOver();
			yield break;
		}

        // Reset all values.
		transform.position = Vector3.zero;
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		transform.rotation = Quaternion.identity;
		
        // Show the player.
		HideAndDisablePlayer(false);

        // Disable collision and flash the player.
		polyCollider.enabled = false;
		deathInvulnTimer = 0;
		StartCoroutine(FlashPlayer());
	}

	/**
	 * This method is called when we teleport or die.
	 */
	void HideAndDisablePlayer(bool value) {
		// Set a flag so we can't interact with the ship again until we're done.
		disabled = value;
		
        // Toggle visibility and collision.
		GetComponent<Renderer>().enabled = !value;
		polyCollider.enabled = !value;
		shield.gameObject.SetActive(!value);
		flame.SetActive(!value);

		// Reset our momentum so we start zeroed out.
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0;
	}

	/**
	 * Flash the player after death to give some visual feedback that he's invlunerable.
	 */
	IEnumerator FlashPlayer() {
		float elapsedTime = 0;
		float flashDuration = 0.2f;

		while (elapsedTime < (deathInvulnDuration - 0.2f)) {
			GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
			elapsedTime += flashDuration;
			yield return new WaitForSeconds(flashDuration);
			if (flashDuration > 0.05f) {
				flashDuration -= 0.01f;
			}
		}

		GetComponent<Renderer>().enabled = true;
	}
	
	/**
	 * The method called when the DynamicObject component registers a collsion.
	 */
	public void Hit() {
        // Recharging shield mode.
		if (healthMode == 2) {
			AudioSource.PlayClipAtPoint(manager.shieldHitClip, Camera.main.transform.position);
			
			shieldRechargeTimer = 0;
			
			if (shieldInvulnTimer < shieldInvulnDuration) {
				return;
			}

			StartCoroutine(shield.FlashShield());
			
			currentShieldStrength -= 5;
			manager.RemoveLife(5);
			if (currentShieldStrength <= 0) {
				AudioSource.PlayClipAtPoint(manager.explosionBigClip, Camera.main.transform.position);
				ParticleSystem particleInstance = Instantiate(manager.explodeParticles, transform.position, transform.rotation) as ParticleSystem;
				Destroy(particleInstance.gameObject, 1f);
				StartCoroutine("Death");
			}
			shieldInvulnTimer = 0;
		}
        // Lives mode.
		else {
			lives--;
			manager.RemoveLife(1);
			AudioSource.PlayClipAtPoint(manager.explosionBigClip, Camera.main.transform.position);
			ParticleSystem particleInstance = Instantiate(manager.explodeParticles, transform.position, transform.rotation) as ParticleSystem;
			Destroy(particleInstance.gameObject, 1f);
			StartCoroutine("Death");
		}
	}
}
