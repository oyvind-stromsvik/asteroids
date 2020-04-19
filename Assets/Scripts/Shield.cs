using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	// Public so the parent can set it.
	[HideInInspector]
	public float shieldInvulnDuration;

	/**
	 * Show and fade out the shield when we get hit.
	 */
	public IEnumerator FlashShield() {
		// Get the color we've defined for the shield material.
		Color newColor = GetComponent<Renderer>().materials[0].GetColor("_Color");
		
		// Set the shield to be fully visible.
		newColor.a = 1;
		GetComponent<Renderer>().materials[0].SetColor("_Color", newColor);
		
		// Fade the alpha towards 0 over the duration we're invlunerable.
		float elapsedTime = 0;
		while (elapsedTime < shieldInvulnDuration) {
			float alpha = Mathf.Lerp(1, 0, elapsedTime / shieldInvulnDuration);
			newColor.a = alpha;
			GetComponent<Renderer>().materials[0].SetColor("_Color", newColor);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Make sure the shield is completely invisble after the fadeout is complete.
		newColor.a = 0;
		GetComponent<Renderer>().materials[0].SetColor("_Color", newColor);
	}
}
