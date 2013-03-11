using UnityEngine;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
	public CharacterSpawnType m_CharacterType = CharacterSpawnType.E_SpawnHero;
	
	public CharacterSpawnType GetCharacterSpawnType() { return m_CharacterType; }
	
	public GameObject SpawnCharacter( bool _SpawnPlayerCharacter )
	{
		GameObject spawnedCharacter = null;
		
		SpawnManager spawnManager = (SpawnManager)FindObjectOfType( typeof(SpawnManager) );
		if (spawnManager != null)
		{
			bool spawnHero = (m_CharacterType == CharacterSpawnType.E_SpawnHero);
			spawnedCharacter = spawnManager.SpawnCharacter(_SpawnPlayerCharacter, spawnHero, gameObject.transform.position, gameObject.transform.rotation);
		}
		
		return spawnedCharacter;
	}
	
	public static CharacterSpawner FindCharacterSpawner( CharacterSpawnType _CharacterType )
	{
		CharacterSpawner matchingSpawner = null;
		
		Object[] spawners = FindObjectsOfType(typeof(CharacterSpawner));
		foreach ( Object spawner in spawners )
		{
			CharacterSpawner characterSpawner = spawner as CharacterSpawner;
			if (characterSpawner.GetCharacterSpawnType() == _CharacterType)
			{
				matchingSpawner = characterSpawner;
				break;
			}
		}
		
		return matchingSpawner;
	}
	
	void Start()
	{
		enabled = false;
	}
	
	void Update()
	{
	
	}
}
