using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public enum MenuState { E_MenuNone, E_MenuMainPanel, E_MenuJoinPanel, E_MenuStartingGame, E_MenuConnectingGame, E_MenuTransitioningToLevel, E_MenuQuitingGame,};

public class MainMenu : MonoBehaviour
{
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private NetworkManager m_NetworkManager = null;
	private PlayerManager m_PlayerManager = null;
	
	private bool m_IsActiveSequence = false;
	private bool IsSequenceActive() { return m_IsActiveSequence; }
	
	private int m_NextLevelIndex = -1;
	
	public int m_CreditSceneIndex = 0;
	public int m_FirstLevelSceneIndex = 0;
	
	public PlayerInputDevice m_PlayerInputDevice = PlayerInputDevice.E_PlayerGamepad_1;
	
	private MenuState m_MenuState = MenuState.E_MenuNone;
	private void ChangeMenuState(MenuState _NewMenuState) { m_MenuState = _NewMenuState; }
	
	private int m_panelCount = 0;
	
	private bool m_enableUI = false;
	bool IsUIEnable() { return m_enableUI; }
	
	//@HACK temporary display solution for server list
	private string[] m_DisplayServerList = null;
	private Vector2 m_ScrollPositionServerSelect = Vector2.zero;
	public int m_SelectedDisplayedServerIndex = -1;
	public float m_ServerListUpdateTimeIntervalInSeconds = 2.0f;
	private float m_TimeSinceLastServerListUpdate = 0.0f;
	
	void Start()
	{
		StartSequence();
		
		if (IsSequenceActive())
		{
			EnableUI();
			ChangeMenuState(MenuState.E_MenuMainPanel);
		}
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
		
		PlayerManager playerManager = (PlayerManager)FindObjectOfType( typeof(PlayerManager) );
		if (playerManager == null)
		{
			Debug.Log("GameManager prefab is missing a PlayerManager component!");
		}
		
		m_LevelManager = levelManager;
		m_NetworkManager = networkManager;
		m_PlayerManager = playerManager;
		
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
	
	private void EnableUI()
	{
		m_enableUI = true;
	}
	
	private void DisableUI()
	{
		m_enableUI = false;
	}
	
	void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	private void QuitGame()
	{
		m_LevelManager.QuitGame();
	}
	
	private void TransitionToLevel(int _LevelIndex)
	{
		m_NextLevelIndex = _LevelIndex;
	}
	
	private void UpdateMenuTransition(float _DeltaTime)
	{
		bool nextLevelIsValid = m_LevelManager.IsValidLevelIndex(m_NextLevelIndex);
		if (nextLevelIsValid)
		{
			NetworkServer networkServer = m_NetworkManager.GetServer();
			if (networkServer == null)
			{
				Debug.Log("UpdateMenuTransition: No Network Server - initialization failed?");
			}
			else if (networkServer.IsServerStarted())
			{
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				PlayerInputDevice localPlayerInput = m_PlayerInputDevice;
				
				if (m_PlayerManager.HasLocalPlayerJoined(localNetClient, localPlayerInput))
				{
					ServerEndSequence();
				}
				else if (m_PlayerManager.IsLocalPlayerJoining(localNetClient, localPlayerInput) == false)
				{
					bool isAuthority = m_NetworkManager.IsNetworkAuthorithy();
					//@FIXME handle the case where the RPC call to server got lost/discarded?
					m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, isAuthority);
				}
			}
		}
		else
		{
			NetworkClient networkClient = m_NetworkManager.GetClient();
			if (networkClient != null && networkClient.IsConnectedToServer())
			{
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				PlayerInputDevice localPlayerInput = m_PlayerInputDevice;
				
				if (m_PlayerManager.HasLocalPlayerJoined(localNetClient, localPlayerInput))
				{
					//@TODO: should client request level load or the server should send it automatically?? => probably server
					//ServerEndSequence();
					m_LevelManager.ClientLoadLevelRequest(localNetClient);
					
					//@FIXME
					m_IsActiveSequence = false;
				}
				else if (m_PlayerManager.IsLocalPlayerJoining(localNetClient, localPlayerInput) == false)
				{
					bool isAuthority = m_NetworkManager.IsNetworkAuthorithy();
					System.Diagnostics.Debug.Assert(isAuthority == false);
					//@FIXME handle the case where the RPC call to server got lost/discarded?
					m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, isAuthority);
				}
				else
				{
					//@TODO: add some time out?
				}
			}
		}
	}
	
	private void UpdateMenuPanels(float _DeltaTime)
	{
		if (IsUIEnable())
		{
			m_panelCount = 1;
			
			if (m_NetworkManager)
			{
				UpdateServerList(_DeltaTime);
			}
			
			if (m_DisplayServerList != null)
			{
				++m_panelCount;
			}
		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			UpdateMenuPanels(deltaTime);
			
			UpdateMenuTransition(deltaTime);
		}
	}
	
	private void CreateGameButtonPressed()
	{
		if (m_NetworkManager != null)
		{
			m_NetworkManager.StopServerSearch();
			m_NetworkManager.StartServer(null, null);
		}
		
		TransitionToLevel(m_FirstLevelSceneIndex);
		
		//@TODO disable menu
		DisableUI();
	}
	
	private void JoinGameButtonPressed()
	{
		if (m_NetworkManager != null)
		{
			m_NetworkManager.StartServerSearch();
			m_TimeSinceLastServerListUpdate = m_ServerListUpdateTimeIntervalInSeconds;
		}
	}
	
	private void AboutGameButtonPressed()
	{
		// About Game
		TransitionToLevel(m_CreditSceneIndex);
		
		//@TODO disable menu
		DisableUI();
	}
	
	private void QuitGameButtonPressed()
	{
		// Exit Game
		QuitGame();
		
		//@TODO disable menu?
		DisableUI();
	}
	
	private void ConnectGameButtonPressed()
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		System.Diagnostics.Debug.Assert(m_SelectedDisplayedServerIndex >= 0);
		
		NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
		if (networkServerSearch != null)
		{
			//@FIXME: technically getting an updated list at that point can be a problem because the selected server index might be affected
			List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
			int serverCount = serverList.Count;
			if (m_SelectedDisplayedServerIndex < serverCount)
			{
				NetworkServerSearch.ServerSearchData server = serverList[m_SelectedDisplayedServerIndex];
				string serverIpAddress = server.m_ServerIpAddressString;
				int connectPort = server.m_ServerConnectPort;
				
				m_NetworkManager.StopServerSearch();
				m_NetworkManager.ConnectToServer(serverIpAddress, connectPort);
			}
			
			//@TODO disable menu
			DisableUI();
		}
	}

	void OnGUI ()
	{
		if (IsUIEnable())
		{
			int panelSlideOffsetX = -200;
			
			int panelWidth = 200;
			int panelHeight = 400;
			
			int centerPanelOffsetX = Screen.width / 2 - panelWidth / 2;
			int centerPanelOffsetY = Screen.height / 2 - panelHeight / 2;
			
			int mainPanelIndex = 1;
			if (m_panelCount >= mainPanelIndex)
			{
				int mainPanelSlideCount = m_panelCount - mainPanelIndex;
				int mainPanelOffsetX = centerPanelOffsetX + mainPanelSlideCount * panelSlideOffsetX;
				int mainPanelOffsetY = centerPanelOffsetY;
				Rect mainPanel = new Rect( mainPanelOffsetX, mainPanelOffsetY, panelWidth, panelHeight);
				
				GUI.Box(mainPanel, "\n<b>You are the villain</b>\n\nExcellent, you've kidnapped\nthe princess!\n\nDon't let the idiot hero\n get her back though...");
				
				int mainPanelButtonWidth = 160;
				int mainPanelButtonHeight = 40;
				int mainPanelButtonOffsetX = mainPanelOffsetX + (panelWidth - mainPanelButtonWidth) / 2;
				int mainPanelButtonOffsetY = mainPanelOffsetY + 160;
				int mainPanelButtonInterspaceY = 60;
				
				string createGameButtonLabel = "Create Game";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), createGameButtonLabel))
				{
					CreateGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
		
				string joinGameButtonLabel = "Join Game";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), joinGameButtonLabel))
				{
					JoinGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
				
				string aboutGameButtonLabel = "About";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), aboutGameButtonLabel))
				{
					AboutGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
		
				string exitGameButtonLabel = "Exit";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), exitGameButtonLabel))
				{
					QuitGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
			}
			
			//@HACK temporary server list display solution
			int joinPanelIndex = 2;
			if (m_panelCount >= joinPanelIndex)
			{
				int joinPanelSlideCount = m_panelCount - joinPanelIndex;
				int joinPanelOffsetX = centerPanelOffsetX + joinPanelSlideCount * panelSlideOffsetX;
				int joinPanelOffsetY = centerPanelOffsetY;
				Rect joinPanel = new Rect( joinPanelOffsetX, joinPanelOffsetY, panelWidth, panelHeight);
				
				GUI.Box(joinPanel, "\n<b>Existing servers:</b>");
				
				int joinPanelTitleHeight = 40;
				
				int joinPanelButtonHeight = 40;
				int joinPanelButtonWidth = 160;
				int joinPanelButtonOffsetX = joinPanelOffsetX + (panelWidth - joinPanelButtonWidth) / 2;
				
				System.Diagnostics.Debug.Assert(m_DisplayServerList != null);
				int serverCount = m_DisplayServerList.Length;
				if (serverCount > 0)
				{
					int joinPanelServerListWidth = 180;
					int joinPanelServerListHeight = 300;
					
					int joinPanelServerListOffsetX = joinPanelOffsetX + (panelWidth - joinPanelServerListWidth) / 2;
					int joinPanelServerListOffsetY = joinPanelOffsetY + joinPanelTitleHeight;
					
					DrawServerList(joinPanelServerListOffsetX, joinPanelServerListOffsetY, joinPanelServerListWidth, joinPanelServerListHeight);
					
					if (m_SelectedDisplayedServerIndex >= 0 && m_SelectedDisplayedServerIndex < serverCount)
					{
						int joinPanelConnectGameButtonOffsetY = joinPanelOffsetY + joinPanelTitleHeight + joinPanelServerListHeight;
						
						string connectGameButtonLabel = "Connect Game";
						if (GUI.Button(new Rect(joinPanelButtonOffsetX, joinPanelConnectGameButtonOffsetY, joinPanelButtonWidth, joinPanelButtonHeight), connectGameButtonLabel))
						{
							ConnectGameButtonPressed();
						}
					}
				}
			}
		}
	}
	
	private void UpdateServerList(float _DeltaTime)
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
		
		if (networkServerSearch != null)
		{
			m_TimeSinceLastServerListUpdate += _DeltaTime;
			if ( m_TimeSinceLastServerListUpdate >= m_ServerListUpdateTimeIntervalInSeconds)
			{
				List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
				if (serverList != null)
				{
					m_DisplayServerList = serverList.Select(x => x.m_ServerName).ToArray();
				}
				else
				{
					string[] emptyList = {};
					m_DisplayServerList = emptyList;
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
	
	private void DrawServerList(int _PanelOffsetX, int _PanelOffsetY, int _PanelWidth, int _PanelHeight)
	{
		//@HACK temporary display solution
		
		Rect scrollViewRect = new Rect(_PanelOffsetX, _PanelOffsetY, _PanelWidth, _PanelHeight);//Screen.width / 2 + 120, Screen.height / 2 - 200, 200, 400);
		Rect scrollAreaRect = new Rect (0, 0, _PanelWidth, _PanelHeight);//0, 0, 200, 400);
		Vector2 scrollPos = m_ScrollPositionServerSelect;
		bool alwaysShowHorizontalScrollBar = false;
		bool alwaysShowVerticalScrollBar = false;
		
		scrollPos = GUI.BeginScrollView(scrollViewRect, scrollPos, scrollAreaRect, alwaysShowHorizontalScrollBar, alwaysShowVerticalScrollBar);
		
			m_SelectedDisplayedServerIndex = GUILayout.SelectionGrid(m_SelectedDisplayedServerIndex, m_DisplayServerList, 1);
		
		GUI.EndScrollView();
	}
}
