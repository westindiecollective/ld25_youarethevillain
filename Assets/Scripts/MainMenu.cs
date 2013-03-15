using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public class MainMenu : MonoBehaviour
{
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private NetworkManager m_NetworkManager = null;
	
	private bool m_IsActiveSequence = false;
	
	private int m_NextLevelIndex = -1;
	
	public int m_CreditSceneIndex = 0;
	public int m_FirstLevelSceneIndex = 0;
	
	//@HACK temporary display solution for server list
	private string[] m_DisplayServerList = null;
	private Vector2 m_ScrollPositionServerSelect = Vector2.zero;
	public int m_SelectedDisplayedServerIndex = -1;
	public float m_ServerListUpdateTimeIntervalInSeconds = 2.0f;
	private float m_TimeSinceLastServerListUpdate = 0.0f;
	
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
		
		NetworkManager networkManager = (NetworkManager)FindObjectOfType( typeof(NetworkManager) );
		if (networkManager == null)
		{
			Debug.Log("GameManager prefab is missing a NetworkManager component!");
		}
		
		m_LevelManager = levelManager;
		m_NetworkManager = networkManager;
		
		m_IsActiveSequence = (m_LevelManager != null);
		
		InitCamera();
	}
	
	void EndSequence()
	{
		m_IsActiveSequence = false;
			
		m_LevelManager.LoadLevel(m_NextLevelIndex);
	}
	
	void ServerEndSequence()
	{
		m_IsActiveSequence = false;
		
		m_LevelManager.ServerLoadLevel(m_NextLevelIndex);
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
		bool nextLevelIsValid = m_LevelManager.IsValidLevelIndex(m_NextLevelIndex);
		if (nextLevelIsValid)
		{
			NetworkServer networkServer = m_NetworkManager.GetServer();
			if (networkServer == null)
			{
				EndSequence();
			}
			else if (networkServer.IsServerStarted())
			{
				ServerEndSequence();
			}

		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			UpdateMenuTransition(deltaTime);
			
			if (m_NetworkManager)
			{
				UpdateServerList(deltaTime);
			}
		}
	}

	void OnGUI ()
	{
		if (m_IsActiveSequence)
		{
			GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 200, 200, 400), "\n<b>You are the villain</b>\n\nExcellent, you've kidnapped\nthe princess!\n\nDon't let the idiot hero\n get her back though...");
			
			string createGameButtonLabel = "Create Game";
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 - 60, 160, 40), createGameButtonLabel))
			{
				// Create Game
				if (m_NetworkManager != null)
				{
					m_NetworkManager.StopServerSearch();
					m_NetworkManager.StartServer(null, null);
				}
				
				TransitionToLevel(m_FirstLevelSceneIndex);
				
				//@TODO disable menu
			}
	
			string joinGameButtonLabel = "Join Game";
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2, 160, 40), joinGameButtonLabel))
			{
				// Join Game
				if (m_NetworkManager != null)
				{
					m_NetworkManager.StartServerSearch();
				}
				//@TODO disable menu
			}
			
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 60, 160, 40), "About"))
			{
				// About Game
				TransitionToLevel(m_CreditSceneIndex);
				
				//@TODO disable menu
			}
	
			if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 120, 160, 40), "Exit"))
			{
				// Exit Game
				QuitGame();
				
				//@TODO disable menu?
			}
			
			//@HACK temporary server list display solution
			if (m_NetworkManager!= null)
			{
				NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
				if ( (networkServerSearch != null) && (m_DisplayServerList != null) && (m_DisplayServerList.Length > 0) )
				{
					DrawServerList();
					
					System.Diagnostics.Debug.Assert(m_SelectedDisplayedServerIndex >= 0);
					
					string connectGameButtonLabel = "Connect Game";
					if (GUI.Button(new Rect(Screen.width / 2 + 120, Screen.height / 2 + 200, 160, 40), connectGameButtonLabel))
					{
						//@FIXME: technically getting an updated list at that point can be a problem because the selected server index might be affected
						List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
						NetworkServerSearch.ServerSearchData server = serverList[m_SelectedDisplayedServerIndex];
						string serverIpAddress = server.m_ServerIpAdress;
						int connectPort = server.m_ServerConnectPort;
						
						m_NetworkManager.StopServerSearch();
						m_NetworkManager.ConnectToServer(serverIpAddress, connectPort);
						
						//@TODO disable menu
					}
				}
			}
		}
	}
	
	private void UpdateServerList(float _deltaTime)
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
		
		if (networkServerSearch != null)
		{
			m_TimeSinceLastServerListUpdate += _deltaTime;
			if ( m_TimeSinceLastServerListUpdate >= m_ServerListUpdateTimeIntervalInSeconds)
			{
				List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
				if (serverList != null)
				{
					m_DisplayServerList = serverList.Select(x => x.m_ServerName).ToArray();
				}
				else
				{
					m_DisplayServerList = null;
				}
				
				m_TimeSinceLastServerListUpdate = 0.0f;
			}
		}
		else
		{
			m_TimeSinceLastServerListUpdate = m_ServerListUpdateTimeIntervalInSeconds;
			m_DisplayServerList = null;
		}
	}
	
	private void DrawServerList()
	{
		//@HACK temporary display solution
		
		Rect scrollViewRect = new Rect(Screen.width / 2 + 120, Screen.height / 2 - 200, 200, 400);
		Rect scrollAreaRect = new Rect (0, 0, 200, 400);
		Vector2 scrollPos = m_ScrollPositionServerSelect;
		bool alwaysShowHorizontalScrollBar = false;
		bool alwaysShowVerticalScrollBar = false;
		
		scrollPos = GUI.BeginScrollView(scrollViewRect, scrollPos, scrollAreaRect, alwaysShowHorizontalScrollBar, alwaysShowVerticalScrollBar);
		
			m_SelectedDisplayedServerIndex = GUILayout.SelectionGrid(m_SelectedDisplayedServerIndex, m_DisplayServerList, 1);
		
		GUI.EndScrollView();
	}
}
