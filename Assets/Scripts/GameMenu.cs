using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	void OnGUI () {
		GUI.Box(new Rect(Screen.width / 2 - 80, Screen.height / 2 - 20, 160, 40), "Paused");
	}
}
