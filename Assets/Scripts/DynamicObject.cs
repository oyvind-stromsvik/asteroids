using UnityEngine;
using System.Collections;

public class DynamicObject : MonoBehaviour {

	// Local variables and references.
	Vector2 screenSize;
	PolygonCollider2D polyCollider;
	BoxCollider2D boxCollider;
	Bounds bounds;

	void Start() {
		screenSize = (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
		polyCollider = GetComponent<PolygonCollider2D>();
		boxCollider = GetComponent<BoxCollider2D>();
	}

	void Update() {
		// Get the bounds of the object.
		// The projectile uses a box collider, the rest use polygon colliders.
		if (polyCollider != null) {
			bounds = polyCollider.bounds;
		}
		else if (boxCollider != null) {
			bounds = boxCollider.bounds;
		}

		// Make sure the object wraps around the edges of the screen correctly.
		if ((transform.position.x - bounds.extents.x) > screenSize.x) {
			transform.position = new Vector3(-screenSize.x - bounds.extents.x, transform.position.y, 0);
		}
		else if ((transform.position.x + bounds.extents.x) < -screenSize.x) {
			transform.position = new Vector3(screenSize.x + bounds.extents.x, transform.position.y, 0);
		}
		else if ((transform.position.y - bounds.extents.y) > screenSize.y) {
			transform.position = new Vector3(transform.position.x, -screenSize.y - bounds.extents.y, 0);
		}
		else if ((transform.position.y + bounds.extents.y) < -screenSize.y) {
			transform.position = new Vector3(transform.position.x, screenSize.y + bounds.extents.y, 0);
		}
	}

	/**
	 * This tells the attached components that they collided. They decide how 
     * to handle the collision with the Hit() method.
	 */
	void OnCollisionEnter2D(Collision2D coll) {
		// If asteroid collision is enabled we skip collisions between asteroids
		// and let them bounce using the physics system instead.
		if (gameObject.tag == "Asteroid" && coll.gameObject.tag == "Asteroid") {
			return;
		}

		// Tell ourself that we got hit.
		SendMessage("Hit");
		// Tell the other gameobject that it was hit as well.
		coll.gameObject.SendMessage("Hit");
    }
}
