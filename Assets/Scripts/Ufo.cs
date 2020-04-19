using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ufo : MonoBehaviour {

    // Our forward speed. Acceleration I guess.
    public float speed;
    // Our rotate speed.
    public float rotationSpeed;

    // The projectile we shoot.
    public Projectile projectile;
    // The speed of the projectiles we shoot.
    public float projectileSpeed;
    // The number of projectiles we can shoot per second.
    public float shotsPerSecond;

    // The points defining the shape of our ship.
	public Vector2[] shipPoints;

    // The number of shield points we have if we're playing with recharging shields.
    public int shieldStrength;
    // The time in seconds we're invulnerable before we can take another hit.
    public float shieldInvulnDuration;

    // The time in seconds before we start correcting ourself if we start spinning.
	public float spinOutDuration;
    // How fast we correct ourself after our angular velocity is zero.
	public float rotationDuration;

    // The current health mode. Public so the game manager can set it.
    [HideInInspector]
    public int healthMode;
    // The UFO type. Public so the game manager can set it.
    [HideInInspector]
    public int ufoType;

	// Local variables and references.
	int currentShieldStrength;
	float shieldInvulnTimer;
    float nextFire;
	DrawObject drawObject;
	PolygonCollider2D polyCollider;
	float turning;
	float thrust;
	List<Vector3> drawPoints;
	GameManager manager;
	GameObject player;
	Shield shield;
	float changeDirectionTimer;
	float changeDirectionInterval;
	float spinOutTimer;
	float rotationTimer;
	float rotationDampening;
    float fireRate;
    int direction;
	
	void Awake() {
		manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		shield = transform.Find("Shield").GetComponent<Shield>();
	}

    void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
		shield.shieldInvulnDuration = shieldInvulnDuration;
		currentShieldStrength = shieldStrength;
        fireRate = 1 / shotsPerSecond;

		// Play a looping sound effect according to our UFO type.
		if (ufoType == 1) {
			GetComponent<AudioSource>().clip = manager.bigUfoClip;
		}
		else {
            GetComponent<AudioSource>().clip = manager.smallUfoClip;
		}
		GetComponent<AudioSource>().Play();

		// Set our direction, velocity and spawn location. We either spawn on the left and
		// travel right or spawn on the right and travel left.
		direction = (Random.value < 0.5f) ? 1 : -1;
		transform.position = new Vector3(-1 * direction * 480, Random.Range(-250f, 250f));
		GetComponent<Rigidbody2D>().velocity = new Vector2(direction * Random.Range(100f, 200f), 0);

		// The interval for when we change directions. This is our "AI". Sometimes it actually
		// doges asteroids or the player and looks intelligent. :p
		changeDirectionInterval = (ufoType == 2) ? Random.Range(0.25f, 1f) : Random.Range(1f, 3f);

		// Set the points on our polygon collider according to the points we've 
		// manually used to define our shape. Apparently this is all you need to 
		// do to get a working polygon collider.
		polyCollider = GetComponent<PolygonCollider2D>();
		polyCollider.points = shipPoints;
		
		// Draw the UFO.
		drawObject = GetComponent<DrawObject>();
		drawPoints = new List<Vector3>();
		for (int i = 0; i < shipPoints.Length; i++) {
			Vector3 pos = new Vector3(shipPoints[i].x, shipPoints[i].y, 0);
			drawPoints.Add(pos);
		}
		drawObject.SetPoints(drawPoints);
		
		// Add one extra line going across the top.
		GameObject line1 = new GameObject();
		line1.transform.parent = transform;
		line1.transform.position = transform.position;
		line1.AddComponent<DrawObject>();
		DrawObject drawObject1 = line1.GetComponent<DrawObject>();
		drawObject1.color = drawObject.color;
		drawObject1.width = drawObject.width;
		List<Vector3> linePoints1 = new List<Vector3>();
		linePoints1.Add(new Vector3(-6, 8, 0) * transform.localScale.x);
		linePoints1.Add(new Vector3(6, 8, 0) * transform.localScale.x);
		drawObject1.SetPoints(linePoints1);
		
		// Add one extra line going across the middle.
		GameObject line2 = new GameObject();
		line2.transform.parent = transform;
		line2.transform.position = transform.position;
		line2.AddComponent<DrawObject>();
		DrawObject drawObject2 = line2.GetComponent<DrawObject>();
		drawObject2.color = drawObject.color;
		drawObject2.width = drawObject.width;
		List<Vector3> linePoints2 = new List<Vector3>();
		linePoints2.Add(new Vector3(-24, 0, 0) * transform.localScale.x);
		linePoints2.Add(new Vector3(24, 0, 0) * transform.localScale.x);
		drawObject2.SetPoints(linePoints2);
    }

	void Update() {
		// Increment all our timers.
        if (healthMode == 2) {
		    shieldInvulnTimer += Time.deltaTime;
        }
		changeDirectionTimer += Time.deltaTime;
		nextFire += Time.deltaTime;
		spinOutTimer += Time.deltaTime;

        // If our rotation is not 0 we've hit something and we'll correct our
        // spin here.
		if (transform.rotation.eulerAngles.z != 0) {
			if (spinOutTimer > spinOutDuration) {
				GetComponent<Rigidbody2D>().angularVelocity -= (Time.deltaTime * rotationDampening);
				rotationDampening += 1;
				if (GetComponent<Rigidbody2D>().angularVelocity <= 0) {
					GetComponent<Rigidbody2D>().angularVelocity = 0;

					transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Lerp(transform.rotation.eulerAngles.z, 0, rotationTimer)));
					rotationTimer += Time.deltaTime / rotationDuration;
					if (rotationTimer >= 1) {
						transform.rotation = Quaternion.identity;
					}
				}
			}
			// If we've rotated it means we've collided and have started to spin.
			// We then exit here so we don't proceed to fire our gun or change directions. 
			// We'll only do that when we've recovered from our spin.
			return;
		}

		rotationDampening = 0;
		spinOutTimer = 0;
		rotationTimer = 0;

		if (nextFire > fireRate) {
			Shoot();
		}

        // Change direction and speed at random intervals.
        // The smaller UFO's do this more often than the larger ones.
		if (changeDirectionTimer > changeDirectionInterval) {
			GetComponent<Rigidbody2D>().velocity = new Vector2(direction * Random.Range(100f, 200f), Random.Range(-100f, 100f));
			changeDirectionTimer = 0;
			changeDirectionInterval = (ufoType == 2) ? Random.Range(0.25f, 1f) : Random.Range(1f, 3f);
		}
	}
    
    /**
     * Shoot a single projectile.
     */
	void Shoot() {
		Quaternion rotation = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.back);
		// If this is the small UFO we aim at the player.
		if (ufoType == 2) {
			rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.back);
			rotation.y = 0;
			rotation.x = 0;
		}
        Projectile instance = Instantiate(projectile, transform.position, rotation) as Projectile;
		instance.GetComponent<Rigidbody2D>().velocity = instance.transform.TransformDirection(0, projectileSpeed, 0);
		Physics2D.IgnoreCollision(GetComponent<Collider2D>(), instance.GetComponent<Collider2D>());
        AudioSource.PlayClipAtPoint(manager.shootClip, Camera.main.transform.position);
		nextFire = 0;
	}

    /**
     * The method called when the DynamicObject component registers a collsion.
     */
	public void Hit() {
        // Recharging shield mode.
		if (healthMode == 2) {
			AudioSource.PlayClipAtPoint(manager.shieldHitClip, Camera.main.transform.position);

			if (shieldInvulnTimer < shieldInvulnDuration) {
				return;
			}

			StartCoroutine(shield.FlashShield());
			
			currentShieldStrength -= 1;
			if (currentShieldStrength <= 0) {
				AudioSource.PlayClipAtPoint(manager.explosionBigClip, Camera.main.transform.position);
				ParticleSystem particleInstance = Instantiate(manager.explodeParticles, transform.position, transform.rotation) as ParticleSystem;
				Destroy(particleInstance.gameObject, 1f);
				
				manager.AddScore(200);
				
				Destroy(gameObject);
			}
			shieldInvulnTimer = 0;
		}
        // Lives mode.
		else {
			AudioSource.PlayClipAtPoint(manager.explosionBigClip, Camera.main.transform.position);
			ParticleSystem particleInstance = Instantiate(manager.explodeParticles, transform.position, transform.rotation) as ParticleSystem;
			Destroy(particleInstance.gameObject, 1f);
			
			manager.AddScore(200);
			
			Destroy(gameObject);
		}
	}
}
