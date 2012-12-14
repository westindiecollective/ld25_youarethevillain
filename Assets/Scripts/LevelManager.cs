using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
	
	private bool m_LoadingLevel = true;
	
	// Use this for initialization
	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	void OnLevelWasLoaded(int _level)
	{
		m_LoadingLevel = false;
	}
	
	void LoadLevel(int _level)
	{
		m_LoadingLevel = true;
		
		Application.LoadLevel(_level);
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}
	
	void OnGUI()
	{
		//Loading screen
		if (m_LoadingLevel)
		{
			
		}
	}
}
