using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	
	private bool m_IsActiveSequence = false;
	
	private int m_NextLevelIndex = -1;
	
	public int m_CreditSceneIndex = 0;
	public int m_FirstLevelSceneIndex = 0;
	
	void Start()
	{
		StartSequence();
	}
	
	void OnLevelWasLoaded(int _level)
	{
		//InitGameSequence();
	}
	
	void StartSequence()
	{
		LevelManager levelManager = (LevelManager)FindObjectOfType( typeof(LevelManager) );
		if (levelManager == null && m_GameManagerPrefab != null)
		{
			GameObject gameManager = Instantiate(m_GameManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			levelManager = gameManager.GetComponent<LevelManager>();
			
			Debug.Log("Instantiating gameManager!");
		}
		
		m_LevelManager = levelManager;
		
		m_IsActiveSequence = (m_LevelManager != null);
		
		InitCamera();
	}
	
	void EndSequence()
	{
		m_IsActiveSequence = false;
			
		m_LevelManager.LoadLevel(m_NextLevelIndex);
	}
	
	void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	void QuitGame()
	{
		m_LevelManager.QuitGame();
	}
	
	void TransitionToLevel(int _LevelIndex)
	{
		m_NextLevelIndex = _LevelIndex;
	}
	
	void UpdateMenuTransition(float _DeltaTime)
	{
		if ( m_LevelManager.IsValidLevelIndex(m_NextLevelIndex) )
		{
			EndSequence();
		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			UpdateMenuTransition(deltaTime);
		}
	}

	void OnGUI ()
	{
		if (m_IsActiveSequence)
		{
			GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 200, 200, 400), "\n<b>You are the villain</b>\n\nExcellent, you've kidnapped\nthe princess!\n\nDon't let the idiot hero\n get her back though...");
			
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2, 160, 40), "Play"))
			{
				// Play Game
				TransitionToLevel(m_FirstLevelSceneIndex);
			}
	
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 60, 160, 40), "About"))
			{
				// About Game
				TransitionToLevel(m_CreditSceneIndex);
			}
	
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 120, 160, 40), "Exit"))
			{
				// Exit Game
				QuitGame();
			}
		}
	}
}
