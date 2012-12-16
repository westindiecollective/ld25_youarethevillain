using UnityEngine;
using System.Collections;

public class Swingable : Useable
{
	public enum SwingableType { E_SwingableNone, E_SwingableBranch };
	SwingableType m_SwingableType;
	
	public void SetSwingableType(SwingableType _SwingableType)
	{
		m_SwingableType = _SwingableType;
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
