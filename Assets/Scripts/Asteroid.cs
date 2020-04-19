using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Asteroid : MonoBehaviour {

	// The initial speed for the asteroid.
    public float initialSpeed;
	// The max number of splits that can occur.
    public int maxNumberOfSplits;
	// The number of children instantiated from a split.
    public int childrenFromSplit;
	// The number of segments in the circle that make up the asteroid.
    public int circleSegments;
	// The radius of the asteroid.
	public float radius;
	// The current split.
	public int currentSplit;

	// Local variables and references.
	int linePoints;
	List<Vector3> drawPoints;
	Vector2[] polyPoints;
    PolygonCollider2D polyCollider;
	DrawObject drawObject;
    GameManager manager;

    void Start() {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

		// Position the points that make up our line renderer and polygon collider such that
		// they create a circle with the radius and number of segments we've specified.
		drawObject = GetComponent<DrawObject>();
		polyCollider = GetComponent<PolygonCollider2D>();
        linePoints = circleSegments + 1;
		polyPoints = new Vector2[linePoints];
		drawPoints = new List<Vector3>();
        float part = 360f / circleSegments;
        int i = 0;
        while (i < linePoints) {
            float duh = part * i * (2 * Mathf.PI / 360) * -1 + Mathf.PI / 2;
            Vector3 pos = new Vector3((Mathf.Cos(duh) * radius), (Mathf.Sin(duh) * radius), 0);

			// Randomize the position of the points a bit on the x and y so we get a more random shape.
			if (i != 0 && i != (linePoints - 1)) {
				pos = new Vector3(pos.x + Random.Range(-radius / 4, radius / 4), pos.y + Random.Range(-radius / 4, radius / 4), 0);
			}

			drawPoints.Add(pos);
			polyPoints[i] = new Vector2(pos.x, pos.y);
            i++;
        }
		drawObject.SetPoints(drawPoints);
		polyCollider.points = polyPoints;

		// Set the velocity to a random amount based on our initial speed.
		GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-initialSpeed, initialSpeed), Random.Range(-initialSpeed, initialSpeed));
		// Increase velocity by 10% per level.
		GetComponent<Rigidbody2D>().velocity *= (0.9f + (manager.currentLevel / 10f));

		// If asteroid collision is enabled then allow the asteroid to rotate.
		GetComponent<Rigidbody2D>().fixedAngle = !manager.asteroidCollision;
    }

    /**
     * The method called when the DynamicObject component registers a collsion.
     */
	public void Hit() {
        // If the asteroid can split then split it into a number of new asteroids.
		if (currentSplit <= maxNumberOfSplits) {
			float part = 360f / childrenFromSplit;
			for (int i = 0; i < childrenFromSplit; i++) {
				float newRadius = radius * 0.5f;
				Vector3 position = transform.position;
				// If asteroid collision is enabled we position each new asteroid as far from each other as possible
				// within the radius of the original asteroid. So with 2 children the first would be a 0 degrees and
				// the second would be at 180 degrees.
				if (manager.asteroidCollision) {
					float duh = part * i * (2 * Mathf.PI / 360) * -1 + Mathf.PI / 2;
					Vector3 pos = new Vector3((Mathf.Cos(duh) * newRadius), (Mathf.Sin(duh) * newRadius), 0);
					position = transform.position + pos;
				}
            	GameObject instance = Instantiate(gameObject, position, transform.rotation) as GameObject;
				// We halve the radius so reduce the mass by 8.
				instance.GetComponent<Rigidbody2D>().mass *= 0.125f;
				// Give the smaller asteroids a potential higher velocity.
				instance.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-100, 100), Random.Range(-100, 100)) * currentSplit;
				instance.GetComponent<Asteroid>().radius = newRadius;
				instance.GetComponent<Asteroid>().currentSplit++;
				manager.AddAsteroid();
			}
		}

        // Play a sound and add score based on how small the asteroid is.
		switch (currentSplit) {
			case 1:
				AudioSource.PlayClipAtPoint(manager.explosionBigClip, Camera.main.transform.position);
				manager.AddScore(20);
				break;
			case 2:
				AudioSource.PlayClipAtPoint(manager.explosionMediumClip, Camera.main.transform.position);
				manager.AddScore(50);
				break;
			case 3:
				AudioSource.PlayClipAtPoint(manager.explosionSmallClip, Camera.main.transform.position);
				manager.AddScore(100);
	            break;
        }

		ParticleSystem particleInstance = Instantiate(manager.explodeParticles, transform.position, transform.rotation) as ParticleSystem;
		Destroy(particleInstance.gameObject, 1f);

        Destroy(gameObject);

        manager.RemoveAsteroid();
	}
}
