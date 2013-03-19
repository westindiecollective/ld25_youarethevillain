#define DEBUG_PLAY_GAME_IN_SLOW_MOTION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState { E_GameLoading, E_GameInitializing, E_GameWaitingForPlayers, E_GamePreparingForStart, E_GamePlaying, E_GamePaused, E_GameEnding, };

public enum GameSpawnCharacterType { E_GameHero, E_GameVillain, };

public class GameChase : MonoBehaviour
{
	public int m_VillainCountMax = 1;
	
	private GameState m_GameState = GameState.E_GameLoading;
	private void ChangeGameState(GameState _NewGameState) { m_GameState = _NewGameState; }
	
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private NetworkManager m_NetworkManager = null;
	private PlayerManager m_PlayerManager = null;
	
	private bool m_IsActiveSequence = false;
	
	private int m_NextLevelIndex = -1;
	
	private bool m_IsGameAuthority = false;
	private bool IsGameAuthority() { return m_IsGameAuthority; }
	
	private bool IsNetworkGame() { return (m_NetworkManager.GetServer() != null) || (m_NetworkManager.GetClient() != null); }
	
	private List<Player> m_WaitingPlayers = null;
	
	public float m_WaitingTimeOutInSeconds = 25.0f;
	private float m_WaitingTime = 0.0f;
	
	public float m_PreparingTimeOutInSeconds = 15.0f;
	private float m_PreparingTime = 0.0f;
	
	public float m_PlayerIsWaitingMessageReSendTimeInSeconds = 0.5f;
	private float m_PlayerIsWaitingMessageTimeSinceLastSend = 0.0f;
	
	public float m_StartGameWaitTimeInSeconds = 10.0f;
	private float m_StartGameWaitTime = 0.0f;
	
	public Transform m_CameraInitFollowTarget = null;
	
#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
	public string m_SlowMotionInputButton = "";
	public float m_SlowMotionTimeScale = 0.5f;
	float m_FixedDeltaTimeRatio = 0.0f;
	public bool m_PlayInSlowMotion = false;
#endif
	
	void Awake()
	{
		
	}
	
	private void InitGameAuthority()
	{
		if (m_NetworkManager.GetServer() != null)
		{
			m_IsGameAuthority = true;
		}
		else if (m_NetworkManager.GetClient() != null)
		{
			m_IsGameAuthority = false;
		}
		else
		{
			//offline / solo mode
			NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
			PlayerInputDevice localPlayerInput = PlayerInputDevice.E_PlayerGamepad_1;
			
			m_IsGameAuthority = true;
			m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, m_IsGameAuthority);
		}
		
		if (IsGameAuthority())
		{
			m_WaitingPlayers = new List<Player>();
		}
	}
	
	void Start()
	{
		StartSequence();
		
#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
		m_FixedDeltaTimeRatio = Time.fixedDeltaTime / Time.timeScale;
		if (m_PlayInSlowMotion)
		{
			Time.timeScale = m_PlayInSlowMotion? m_SlowMotionTimeScale : 1.0f;
			Time.fixedDeltaTime = m_FixedDeltaTimeRatio * Time.timeScale;
		}
#endif
		InitGameAuthority();
		
		WaitForPlayers();
	}
	
	void OnLevelWasLoaded(int _level)
	{
		Debug.Log("Game Chase Sequence: Level is loaded");
		
		LoadingFinished();
	}
	
	void StartSequence()
	{
		Debug.Log("Starting Game Chase Sequence");
		
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
	
	void ServerEndSequence()
	{
		m_IsActiveSequence = false;
			
		m_LevelManager.ServerLoadLevel(m_NextLevelIndex);
	}
	
	void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.GetComponent<ThirdPersonCamera>();
		if (thirdPersonCamera == null)
		{
			thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
			thirdPersonCamera.m_DistanceAway = 12;
			thirdPersonCamera.m_DistanceUp = 5;
			thirdPersonCamera.m_SmoothFactor = 3.0f;
			thirdPersonCamera.m_UseLookDir = true;
			thirdPersonCamera.m_UseFollowPositionMask = true;
			thirdPersonCamera.m_LookDir.x = 0.0f;
			thirdPersonCamera.m_LookDir.y = 0.0f;
			thirdPersonCamera.m_LookDir.z = 1.0f;
			thirdPersonCamera.m_FollowPositionMask.x = 0.0f;
			thirdPersonCamera.m_FollowPositionMask.y = 0.0f;
			thirdPersonCamera.m_FollowPositionMask.z = 1.0f;
		}
		
		if ( m_CameraInitFollowTarget != null )
		{
			thirdPersonCamera.SetFollowTarget(m_CameraInitFollowTarget);
		}
	}
	
	void UpdateCamera()
	{
		//@TODO: improve camera update to adapt to multiple local players
		
		Camera mainCamera = Camera.main;
		ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.GetComponent<ThirdPersonCamera>();
		
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		
		List<Player> players = m_PlayerManager.GetPlayers();
		foreach(Player player in players)
		{
			GameObject playerInstance = player.m_PlayerInstance;
			bool isLocalPlayer = (player.m_NetClient == localNetClient);
			
			if (playerInstance != null && isLocalPlayer)
			{
				thirdPersonCamera.SetFollowTarget(playerInstance.transform);
			}
		}
	}
	
	void LoadingFinished()
	{
		ChangeGameState(GameState.E_GameInitializing);
	}
	
	void WaitForPlayers()
	{
		ChangeGameState(GameState.E_GameWaitingForPlayers);
		
		LocalPlayersAreWaiting();
	}
	
	void LocalPlayersAreWaiting()
	{
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		
		List<Player> localPlayers = m_PlayerManager.FindPlayersFromClient(localNetClient);
		System.Diagnostics.Debug.Assert(localPlayers.Count > 0);
		
		foreach(Player localPlayer in localPlayers)
		{
			if (localPlayer.m_PlayerInstance == null)
			{
				if (IsGameAuthority())
				{
					PlayerIsWaitingAuthority(localPlayer.m_NetClient, localPlayer.m_NetViewID);
				}
				else
				{
					Debug.Log("Sending RPC PlayerIsWaitingAuthority for player " + localPlayer.m_NetViewID.ToString());
					networkView.RPC("PlayerIsWaitingAuthority", RPCMode.Server, localPlayer.m_NetClient, localPlayer.m_NetViewID);
				}
			}
		}
	}
	
	[RPC]
	void PlayerIsWaitingAuthority(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Player playerWaiting = m_PlayerManager.FindPlayer(_NetViewID);
		if (playerWaiting != null && playerWaiting.m_PlayerInstance == null)
		{
			if (m_WaitingPlayers.Contains(playerWaiting) == false)
			{
				m_WaitingPlayers.Add(playerWaiting);
			}
			else
			{
				Debug.Log("Trying to add a player waiting for than once!");
			}
		}
		else
		{
			//@TODO: error message
			//maybe somehow check it's from a player that left / disconnected?
			Debug.Log("Trying to add a player waiting which isn't listed/connected anymore!");
		}
	}
	
	void PrepareForStartAuthority()
	{
		ChangeGameState(GameState.E_GamePreparingForStart);
		
		SpawnWaitingPlayers(m_PlayerManager.GetPlayers(), m_WaitingPlayers, m_VillainCountMax);
		
		//@TODO spawn AIs / NPCs
		
		m_WaitingPlayers.Clear();
	}
	
	void UpdateSpawnAuthority()
	{
		List<Player> players = m_PlayerManager.GetPlayers();
		
		foreach (Player waitingPlayer in m_WaitingPlayers)
		{
			//@FIXME: this will be a problem when it's in fact adding another local player on the client
			//which has already spawned other players
			SpawnPlayersOnClient(players, waitingPlayer.m_NetClient);
		}
		
		SpawnWaitingPlayers(players, m_WaitingPlayers, m_VillainCountMax);
		
		//@TODO spawn AIs / NPCs
	}
	
	void StartGameOnClientAuthority(NetworkPlayer _NetClient)
	{
		bool isNetworkGame = IsNetworkGame();
		if (isNetworkGame)
		{
			networkView.RPC("StartGame", _NetClient);
		}
		else
		{
			StartGame();
		}
	}
	
	void StartGameAuthority()
	{
		ChangeGameState(GameState.E_GamePlaying);
		
		bool isNetworkGame = IsNetworkGame();
		if (isNetworkGame)
		{
			networkView.RPC("StartGame", RPCMode.All);
		}
		else
		{
			StartGame();
		}
	}
	
	[RPC]
	void StartGame()
	{
		Debug.Log("StartGame");
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		
		List<Player> players = m_PlayerManager.GetPlayers();
		foreach(Player player in players)
		{
			bool isLocalPlayer = (player.m_NetClient == localNetClient);
			GameObject playerInstance = player.m_PlayerInstance;
			
			if (isLocalPlayer && playerInstance != null)
			{
				GamePlayerController playerController = playerInstance.GetComponent<GamePlayerController>();
				System.Diagnostics.Debug.Assert(playerController != null);
				
				playerController.EnablePlayerControllerUpdate();
				
				FollowLane followLaneBehaviour = playerInstance.GetComponent<FollowLane>();
				System.Diagnostics.Debug.Assert(followLaneBehaviour != null);
				
				followLaneBehaviour.EnableFollowLaneUpdate();
			}
		}
		
		//@TODO - Enable NPC update
		
		UpdateCamera();
	}
	
	[RPC]
	void UpdateGame()
	{
		
	}
	
	void QuitGame()
	{
		ChangeGameState(GameState.E_GameEnding);
		
		m_LevelManager.QuitGame();
	}
	
	void TransitionToLevel(int _LevelIndex)
	{
		m_NextLevelIndex = _LevelIndex;
	}
	
	void UpdateGameLoading()
	{
		System.Diagnostics.Debug.Assert(false);
	}
	
	bool AreAllPlayersWaitingForServer()
	{
		bool allPlayersWaiting = true;
		
		List<Player> players = m_PlayerManager.GetPlayers();
		System.Diagnostics.Debug.Assert(players.Count > 0);
		
		foreach(Player player in players)
		{
			bool playerIsWaiting = false;
			foreach(Player waitingPlayer in m_WaitingPlayers)
			{
				if (player.m_NetViewID == waitingPlayer.m_NetViewID)
				{
					playerIsWaiting = true;
					break;
				}
			}
			
			if (playerIsWaiting == false)
			{
				allPlayersWaiting = false;
				break;
			}
		}
		
		return allPlayersWaiting;
	}
	
	void UpdateGameWaitingForPlayers(float _DeltaTime)
	{
		if (IsGameAuthority())
		{
			//check if all players are waiting
			//then PrepareForStart()
			m_WaitingTime += _DeltaTime;
			bool waitingTimedOut = (m_WaitingTime >= m_WaitingTimeOutInSeconds);
			if (AreAllPlayersWaitingForServer() || waitingTimedOut)
			{
				PrepareForStartAuthority();
			}
		}
		else
		{
			//frequently tell server about potential local players waiting
			
			m_PlayerIsWaitingMessageTimeSinceLastSend += _DeltaTime;
			bool sendPlayerIsWaitingMessage = (m_PlayerIsWaitingMessageTimeSinceLastSend > m_PlayerIsWaitingMessageReSendTimeInSeconds);
			if (sendPlayerIsWaitingMessage)
			{
				LocalPlayersAreWaiting();
				m_PlayerIsWaitingMessageTimeSinceLastSend = 0.0f;
			}
		}
	}
	
	void UpdateGamePreparingForStart(float _DeltaTime)
	{
		if (IsGameAuthority())
		{
			UpdateSpawnAuthority();
			m_WaitingPlayers.Clear();
			
			//check if all players are ready?
			//then StartGame() (after a count down?)
			
			m_StartGameWaitTime += _DeltaTime;
			bool startGame = (m_StartGameWaitTime >= m_StartGameWaitTimeInSeconds);
			if (startGame)
			{
				StartGameAuthority();	
			}
		}
		else
		{
			//client need to be in that state to stop resending "player is waiting" message to server
		}
	}
	
	void UpdateGamePlaying(float _DeltaTime)
	{
		if (IsGameAuthority())
		{
			UpdateSpawnAuthority();
			
			//@FIXME: this will be a problem when it's in fact starting a game on a client which has already started
			foreach (Player waitingPlayer in m_WaitingPlayers)
			{
				StartGameOnClientAuthority(waitingPlayer.m_NetClient);
			}
			
			m_WaitingPlayers.Clear();
		}
		else
		{
		}
	}
	
	void UpdateGamePaused(float _DeltaTime)
	{
		if (IsGameAuthority())
		{
		}
		else
		{
		}
	}
	
	void UpdateGameEnding(float _DeltaTime)
	{
		if (IsGameAuthority())
		{
		}
		else
		{
		}
	}
	
	void UpdateNetworkConnection()
	{
		//@TODO: check network connection is fine
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			UpdateNetworkConnection();
			
#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
			bool switchSlowMotionMode = (m_SlowMotionInputButton.Length > 0) && Input.GetButtonDown(m_SlowMotionInputButton);
			if (switchSlowMotionMode)
			{
				m_PlayInSlowMotion = !m_PlayInSlowMotion;
				Time.timeScale = m_PlayInSlowMotion? m_SlowMotionTimeScale : 1.0f;
				Time.fixedDeltaTime = m_FixedDeltaTimeRatio * Time.timeScale;
			}
#endif
			switch (m_GameState)
			{
			case GameState.E_GameLoading:
				break;
			case GameState.E_GameWaitingForPlayers:
				UpdateGameWaitingForPlayers(deltaTime);
				break;
			case GameState.E_GamePreparingForStart:
				UpdateGamePreparingForStart(deltaTime);
				break;
			case GameState.E_GamePlaying:
				UpdateGamePlaying(deltaTime);
				break;
			case GameState.E_GamePaused:
				UpdateGamePaused(deltaTime);
				break;
			case GameState.E_GameEnding:
				UpdateGameEnding(deltaTime);
				break;
			}
		}
	}
	
	bool IsPlayerVillain(GameObject _PlayerInstance)
	{
		bool isVillain = (_PlayerInstance != null) && (_PlayerInstance.GetComponent<VillainAnimController>() != null);
		return isVillain;
	}
	
	void SpawnPlayersOnClient(List<Player> players, NetworkPlayer _NetClient)
	{
		foreach (Player player in players)
		{
			GameObject playerInstance = player.m_PlayerInstance;
			if (playerInstance != null)
			{
				bool isVillain = IsPlayerVillain(playerInstance);
				GameSpawnCharacterType spawnType = (isVillain)? GameSpawnCharacterType.E_GameVillain : GameSpawnCharacterType.E_GameHero;
				
				Transform spawnTransform = playerInstance.transform;
				int spawnTypeInt = (int)spawnType;
				
				networkView.RPC("SpawnPlayer", _NetClient, player.m_NetViewID, spawnTypeInt, spawnTransform.position, spawnTransform.rotation);
			}
		}
	}
	
	void SpawnWaitingPlayers(List<Player> players, List<Player> waitingPlayers, int _VillainCountMax)
	{
		System.Diagnostics.Debug.Assert(IsGameAuthority());
		
		int waitingPlayerCount = waitingPlayers.Count;
		if (waitingPlayerCount > 0)
		{		
			int villainCount = 0;
			
			foreach (Player player in players)
			{
				bool isVillain = IsPlayerVillain(player.m_PlayerInstance);
				if (isVillain)
				{
					++villainCount;	
				}
			}
			System.Diagnostics.Debug.Assert(villainCount <= _VillainCountMax);
			
			foreach (Player waitingPlayer in waitingPlayers)
			{
				//@FIXME get player character type from player profile?
				bool spawnVillain = (villainCount < _VillainCountMax);
				GameSpawnCharacterType spawnType = (spawnVillain)? GameSpawnCharacterType.E_GameVillain : GameSpawnCharacterType.E_GameHero;
				
				CharacterSpawner spawner = CharacterSpawner.FindCharacterSpawner(spawnType);
				if (spawner != null)
				{
					Transform spawnTransform = spawner.gameObject.transform;
					int spawnTypeInt = (int)spawnType;
					
					SpawnPlayer(waitingPlayer.m_NetViewID, spawnTypeInt, spawnTransform.position, spawnTransform.rotation);
					networkView.RPC("SpawnPlayer", RPCMode.Others, waitingPlayer.m_NetViewID, spawnTypeInt, spawnTransform.position, spawnTransform.rotation);
					villainCount = spawnVillain? villainCount+1 : villainCount;
				}
				else
				{
					Debug.Log("Failed to spawn player " + waitingPlayer.m_NetViewID.ToString() + ": coudln't find spawner for character type " + spawnType.ToString());	
				}
			}
		}
	}
	
	public GameObject m_VillainPlayerPrefab = null;
	public GameObject m_HeroPlayerPrefab = null;
	public GameObject m_VillainAIPrefab = null;
	public GameObject m_HeroAIPrefab = null;
	
	[RPC]
	public void SpawnPlayer(NetworkViewID _NetViewID, int _SpawnType, Vector3 _Position, Quaternion _Rotation)
	{
		Player playerToSpawn = m_PlayerManager.FindPlayer(_NetViewID);
		System.Diagnostics.Debug.Assert(playerToSpawn != null);
		
		//@NOTE: this is a safeguard which is useful when another local player get added after the game has already started
		//and other players have already been spawned on the client
		if (playerToSpawn.m_PlayerInstance == null)
		{
			GameSpawnCharacterType spawnType = (GameSpawnCharacterType)_SpawnType;
			bool spawnHero = (spawnType == GameSpawnCharacterType.E_GameHero);
			GameObject playerInstance = InstantiatePlayer(spawnHero, _Position, _Rotation);
			
			NetworkView playerNetworkView = playerInstance.GetComponent<NetworkView>();
			//@NOTE: it seems Unity3d RPC system doesn't support assert
			//System.Diagnostics.Debug.Assert(playerNetworkView != null);
			
			playerNetworkView.viewID = _NetViewID;
			
			playerToSpawn.m_PlayerInstance = playerInstance;
			
			Debug.Log("Spawned player: " + _NetViewID.ToString());
		}
		else
		{
			Debug.Log("Trying to spawn player multiple times: " + _NetViewID.ToString());
		}
	}
	
	public GameObject InstantiatePlayer(bool _SpawnHero, Vector3 _Position, Quaternion _Rotation)
	{
		GameObject playerPrefab = _SpawnHero? m_HeroPlayerPrefab : m_VillainPlayerPrefab;
		GameObject playerInstance = Instantiate(playerPrefab, _Position, _Rotation) as GameObject;
		
		return playerInstance;
	}
	
	public GameObject InstantiateAI(bool _SpawnHero, Vector3 _Position, Quaternion _Rotation)
	{
		GameObject AIPrefab = _SpawnHero? m_HeroAIPrefab : m_VillainAIPrefab;
		GameObject spawnedAI = Instantiate(AIPrefab, _Position, _Rotation) as GameObject;
		
		return spawnedAI;
	}
}
