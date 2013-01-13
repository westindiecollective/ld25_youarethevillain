using UnityEngine;
using System.Collections;

public class Fireable : Useable
{
	public enum FireableType { E_FireableNone, E_FireablePoisonedBerries, E_FireableDragonCub };
	FireableType m_FireableType;

	public void SetFireableType(FireableType _FireableType)
	{
		m_FireableType = _FireableType;
	}

	public override UseableType GetUseableType()
	{
		return UseableType.E_UseableFire;
	}

	void Start()
	{
	}

	void Update()
	{
	}

	public override void Use()
	{
		//spawn projectile object
	}
}
