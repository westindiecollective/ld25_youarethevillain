using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	void OnGUI ()
	{
		GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 200, 200, 400), "\n<b>You are the villain</b>\n\nExcellent, you've kidnapped\nthe princess!\n\nDon't let the idiot hero\n get her back though...");
		
		if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2, 160, 40), "Play"))
		{
			// Play Game
		}

		if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 60, 160, 40), "About"))
		{
			// About Game
		}

		if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 120, 160, 40), "Exit"))
		{
			// Exit Game
		}
	}
}
