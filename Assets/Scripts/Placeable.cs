using UnityEngine;
using System.Collections;

public class Placeable : Useable
{
	public enum PlaceableType { E_PlaceableNone, E_PlaceableDemonFlower };
	PlaceableType m_PlaceableType;
	
	public void SetPlaceableType(PlaceableType _PlaceableType)
	{
		m_PlaceableType = _PlaceableType;
	}
	
	public override UseableType GetUseableType()
	{
		return UseableType.E_UseablePlace;
	}
	
	void Start()
	{
		
	}
	
	void Update()
	{
	}
	
	public override void Use()
	{
		//spawn obstacle object
	}
}
