using UnityEngine;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
	public GameSpawnCharacterType m_CharacterType = GameSpawnCharacterType.E_GameHero;
	
	public GameSpawnCharacterType GetCharacterSpawnType() { return m_CharacterType; }
	
	public static CharacterSpawner FindCharacterSpawner( GameSpawnCharacterType _CharacterType )
	{
		CharacterSpawner matchingSpawner = null;
		
		Object[] spawners = FindObjectsOfType(typeof(CharacterSpawner));
		foreach ( Object spawner in spawners )
		{
			CharacterSpawner characterSpawner = spawner as CharacterSpawner;
			GameSpawnCharacterType characterSpawnerType = characterSpawner.GetCharacterSpawnType();
			
			if (characterSpawnerType == _CharacterType)
			{
				matchingSpawner = characterSpawner;
				break;
			}
		}
		
		return matchingSpawner;
	}
	
	public static CharacterSpawner FindCharacterSpawnerAny( bool _PickRandomly )
	{
		CharacterSpawner spawner = null;
		
		Object[] spawners = FindObjectsOfType(typeof(CharacterSpawner));
		
		int spawnerCount = spawners.Length;
		if (spawnerCount > 0)
		{
			int spawnerIndex = 0;
			if (_PickRandomly)
			{
				spawnerIndex = UnityEngine.Random.Range(0, spawnerCount);
			}
			spawner = spawners[spawnerIndex] as CharacterSpawner;
		}

		return spawner;
	}
	
	void Start()
	{
		enabled = false;
	}
	
	void Update()
	{
	
	}
}
