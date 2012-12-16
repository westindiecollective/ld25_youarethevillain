using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

	void OnGUI () {
		GUI.Box(new Rect(Screen.width / 2 - 40, Screen.height - 100, 80, 80), "Goat");
	}
}
