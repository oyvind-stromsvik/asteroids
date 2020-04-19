using UnityEngine;
using System.Collections;

public class ServerScore : MonoBehaviour {

	// The url for the script that adds the score to the database.
	public string addScoreUrl; 
	// The url for the script that retrieves the current high score from the database.
	public string highscoreUrl; 
    // Secret key. Must match the secret key defined on the server.
    public string secretKey; 

	// Local variables and references.
	GameManager manager;

	void Awake() {
		manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
	}

	void Start() {
        // Attempt to get the current high score when the game starts.
        StartCoroutine(DownloadHighscore());
	}

	/**
	 * This sends the player's name and score as well as an unique hash to the 
     * server side script. The server side script then compares the hashes to 
     * verify that the request came from our game and not someplace else and
     * then adds the score to the database.
	 */
	public IEnumerator UploadScore(string name, int score) {
		// Create the security hash using our secret key.
		string hash = Md5Sum(name + score.ToString() + secretKey);

		// Post the URL to the site and create a download object to get the result.
		WWW www = new WWW(addScoreUrl + "?name=" + name + "&score=" + score + "&hash=" + hash);

		// Wait until the upload is done.
		yield return www;

		// If the upload succeeded tell the manager.
		if (www.error == null) {
			manager.ScoreUploaded();
		}
		// Otherwise print the error.
		else {
			print(www.error);
		}
	}

	/**
	 * Get the high score from the server.
	 */
	IEnumerator DownloadHighscore() {
        // Create the security hash using our secret key.
        string hash = Md5Sum(secretKey);

        // Post the URL to the site and create a download object to get the result.
        WWW www = new WWW(highscoreUrl + "?hash=" + hash);

        // Wait until the download is done.
        yield return www; 

		// If the download succeeded display the current high score.
		if (www.error == null) {
            int highscore = System.Convert.ToInt32(www.text);
			manager.DisplayHighscore(highscore);
		} 
		// Otherwise print the error.
		else {
			print(www.error);
		}
	}

	/**
	 * Helper method for creating a md5 hash.
	 */
	string Md5Sum(string strToEncrypt) {
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// Encrypt bytes.
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16).
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++) {
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
}
