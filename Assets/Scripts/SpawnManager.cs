using UnityEngine;
using System.Collections;

public enum CharacterSpawnType { E_SpawnHero, E_SpawnVillain, };

public class SpawnManager : MonoBehaviour
{
	public GameObject m_VillainPlayerPrefab = null;
	public GameObject m_HeroPlayerPrefab = null;
	public GameObject m_VillainAIPrefab = null;
	public GameObject m_HeroAIPrefab = null;
	
	public GameObject SpawnCharacter( bool _SpawnPlayer, bool _SpawnHero, Vector3 _Position, Quaternion _Rotation )
	{
		GameObject characterPrefab = null;
		if (_SpawnPlayer && _SpawnHero)
		{
			characterPrefab = m_HeroPlayerPrefab;
		}
		else if (_SpawnPlayer && !_SpawnHero)
		{
			characterPrefab = m_VillainPlayerPrefab;
		}
		else if (!_SpawnPlayer && _SpawnHero)
		{
			characterPrefab = m_HeroAIPrefab;
		}
		else
		{
			characterPrefab = m_VillainAIPrefab;
		}
		
		GameObject spawnedCharacter = Instantiate(characterPrefab, _Position, _Rotation) as GameObject;
		
		return spawnedCharacter;
	}
	
	void Start()
	{
		
	}
	
	void Update()
	{
	
	}
}
