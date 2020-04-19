using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {

	// The duration in seconds before we destroy the projectile if it hasn't collided with anything.
    public float life;

	// If the player fired this projectile we assign him here so we can subtract from
	// his active projectile count when the projectile is destroyed.
	// Public so the player can set it.
	[HideInInspector]
	public Player player;

	// Local variables and references.
	DrawObject drawObject;
	List<Vector3> drawPoints;

    void Start() {
		// Draw the projectile as a line with 2 points.
		drawObject = GetComponent<DrawObject>();
		drawPoints = new List<Vector3>();
		drawPoints.Add(new Vector3(0, 4, 0));
		drawPoints.Add(new Vector3(0, -4, 0));
		drawObject.SetPoints(drawPoints);

		// Destroy the projectile in "life" seconds.
		Invoke("Hit", life);
    }

	/**
	 * The method called when the DynamicObject component registers a collsion.
	 */
	public void Hit() {
		// If this projectile was fire by the player subtract from the player's active 
		// projectile count so he's allowed to fire again if he's at the limit.
		if (player != null) {
			player.activeProjectiles--;
		}

		Destroy(gameObject);
	}
}
