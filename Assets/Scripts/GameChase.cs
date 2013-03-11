using UnityEngine;
using System.Collections;

public class GameChase : MonoBehaviour
{
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	
	private bool m_IsActiveSequence = false;
	
	private int m_NextLevelIndex = -1;
	
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
		
		SpawnCharacters();
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
		ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
		
		//@FIXME get player character type from player profile?
		CharacterSpawnType playerCharacterType = CharacterSpawnType.E_SpawnVillain;
		CharacterSpawner playerSpawner = CharacterSpawner.FindCharacterSpawner(playerCharacterType);
		
		if ( playerSpawner )
		{
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
			thirdPersonCamera.SetFollowTarget(playerSpawner.gameObject.transform);
		}
	}
	
	void SpawnCharacters()
	{
		//@TEMP characters/players will have to be spawned by network authority
		//@FIXME get player character type from player profile?
		CharacterSpawnType playerCharacterType = CharacterSpawnType.E_SpawnVillain;
		CharacterSpawner playerSpawner = CharacterSpawner.FindCharacterSpawner(playerCharacterType);
		
		if ( playerSpawner )
		{
			bool spawnPlayerCharacter = true;
			GameObject spawnedPlayer = playerSpawner.SpawnCharacter(spawnPlayerCharacter);
			
			Camera mainCamera = Camera.main;
			ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.GetComponent<ThirdPersonCamera>();
			
			thirdPersonCamera.SetFollowTarget(spawnedPlayer.transform);
		}
	}
	
	void QuitGame()
	{
		m_LevelManager.QuitGame();
	}
	
	void TransitionToLevel(int _LevelIndex)
	{
		m_NextLevelIndex = _LevelIndex;
	}
	
	void Update()
	{
		//float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			
		}
	}

	void OnGUI ()
	{
		if (m_IsActiveSequence)
		{

		}
	}
}
