using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
	
	private bool m_LoadingLevel = true;
	public int m_LevelCount = 1;
	
	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	void OnLevelWasLoaded(int _level)
	{
		m_LoadingLevel = false;
		
		Debug.Log( string.Format("Loaded level {0} ...", _level) );
	}
	
	public void LoadLevel(int _level)
	{
		int levelToLoad = _level % m_LevelCount;
		
		Debug.Log( string.Format("Loading level {0} ...", levelToLoad) );
		
		m_LoadingLevel = true;

		Application.LoadLevel(levelToLoad);
	}
	
	public void LoadNextLevel()
	{
		int nextLevel = (Application.loadedLevel + 1) % m_LevelCount;
		LoadLevel(nextLevel);
	}
	
	public IEnumerator LoadLevelAsync(int _level, int _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);
		
		LoadLevel(_level);
	}
	
	public IEnumerator LoadNextLevelAsync(int _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);
		
		LoadNextLevel();
	}
	
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
