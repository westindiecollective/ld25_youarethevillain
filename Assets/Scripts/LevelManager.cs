using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{

	private bool m_LoadingLevel = true;
	
	public bool IsLoading()
	{
		return m_LoadingLevel;
	}
	
	public bool IsValidLevelIndex(int _levelIndex)
	{
		bool IsValidIndex = (0 <= _levelIndex) && (_levelIndex < Application.levelCount);
		return IsValidIndex;
	}
	
	public void QuitGame()
	{
		Application.Quit();
	}
	
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
		int levelToLoad = _level % Application.levelCount;

		Debug.Log( string.Format("Loading level {0} ...", levelToLoad) );

		m_LoadingLevel = true;

		Application.LoadLevel(levelToLoad);
	}

	public void LoadNextLevel()
	{
		int nextLevel = (Application.loadedLevel + 1) % Application.levelCount;
		LoadLevel(nextLevel);
	}

	public IEnumerator LoadLevelAsync(int _level, float _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);

		LoadLevel(_level);
	}

	public IEnumerator LoadNextLevelAsync(float _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);

		LoadNextLevel();
	}

	void Update()
	{
	}
}
