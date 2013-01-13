using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	private bool m_GamePaused = false;

	void Update ()
	{
		if(Input.GetKeyDown("escape"))
		{
			if (m_GamePaused)
			{
				m_GamePaused = false;
				//AudioListener.volume = 1;
				Time.timeScale = 1;
				//Screen.showCursor = false;
			}
			else
			{
				m_GamePaused = true;
				//AudioListener.volume = 0;
				Time.timeScale = 0;
				//Screen.showCursor = true;				
			}
		}
	}

	void OnGUI ()
	{
		if (m_GamePaused)
		{
			GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 120), "Paused");

			if (GUI.Button (new Rect (Screen.width / 2 - 90, Screen.height / 2 + 30, 180, 50), "Quit Game"))
			{
				Application.Quit();
			}
		}
	}
}
