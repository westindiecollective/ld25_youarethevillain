using UnityEngine;
using System.Collections;

public class Throwable : Useable
{
	public enum ThrowableType { E_ThrowableNone, E_ThrowableSnake, E_ThrowableSquirrel, E_ThrowableVampireBat, E_ThrowableLavaStone };
	ThrowableType m_ThrowableType;
	
	public void SetThrowableType(ThrowableType _ThrowableType)
	{
		m_ThrowableType = _ThrowableType;
	}
	
	public override UseableType GetUseableType()
	{
		return UseableType.E_UseableThrow;	
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
