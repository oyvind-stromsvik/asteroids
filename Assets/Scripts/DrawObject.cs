using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawObject : MonoBehaviour {

	// The color to draw with.
    public Color color;
    // The width of the line.
	public float width;
	// A bool indicating if we should draw double or not. With this set to
	// true we draw one additional time going through all the points in reverse order.
	// The line renderer can twist a lot so it's hard to get a nice thick line, but by
	// doing this we're remedying it a lot.
	public bool drawDouble = true;
	
	// Local variables and references.
    LineRenderer lineRenderer;
    Vector2 screenSize;
    int vertexCount;
    GameObject go;
	List<Vector3> drawPoints;
	GameManager manager;

    void Awake() {
		manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (!GetComponent<LineRenderer>()) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
		else {
			lineRenderer = GetComponent<LineRenderer>();
		}
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.useWorldSpace = false;
    }

	void Start() {
		if (manager.blackAndWhite) {
			lineRenderer.SetColors(Color.white, Color.white);
		}
		else {
			lineRenderer.SetColors(color, color);
		}
		lineRenderer.SetWidth(width, width);
	}

	/**
	 * This method sets the points that the line renderer will use.
	 */
	public void SetPoints(List<Vector3> points) {
		drawPoints = new List<Vector3>();
		foreach (Vector3 point in points) {
			drawPoints.Add(point);
		}

		// If double draw is turned on add all the points once more in reverse order.
		if (drawDouble) {
			points.Reverse();
			// Skip the first (last) point because it was already added 
            // in the original foreach loop.
			points.RemoveAt(0);
			foreach (Vector3 point in points) {
				drawPoints.Add(point);
			}
		}

		lineRenderer.SetVertexCount(drawPoints.Count);

		int i = 0;
		foreach (Vector3 point in drawPoints) {
			lineRenderer.SetPosition(i, point);
			i++;
		}
	}
}
