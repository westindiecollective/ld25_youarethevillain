using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour
{
	public string m_UseItemButton = "Fire2";
	
	public int m_SwingCountPerBranch = -1;
	public float m_FireDurationPerDragonCub = 10.0f;
	
	public bool m_DropPreviousIfNewPickup = false;
	
	Useable m_ItemToUse;
	int m_ItemUseCount;
	
	void Start()
	{
	
	}
	
	void Update()
	{
		bool useItem = Input.GetButton(m_UseItemButton);
		if ( useItem && m_ItemToUse )
		{
			Use(m_ItemToUse);
			
			if ( m_ItemUseCount == 0 )
			{
				RemoveUsable();
			}
        }
	}
	
	public bool AddPickup(Pickable.PickupType _PickupType, int _PickupCount)
	{
		bool pickedup = AddUseableFromPickup(_PickupType, _PickupCount);
		
		return pickedup;
	}
	
	bool AddUseableFromPickup(Pickable.PickupType _PickupType, int _PickupCount)
	{
		bool added = false;
		
		if ( m_DropPreviousIfNewPickup )
		{
			RemoveUsable();
		}
		
		if ( m_ItemToUse == null )
		{
			//add useable component accordingly
			switch (_PickupType)
			{			
			case Pickable.PickupType.E_PickupBranch:
				m_ItemToUse = AddSwingable(Swingable.SwingableType.E_SwingableBranch);
				m_ItemUseCount = _PickupCount * m_SwingCountPerBranch;
				break;
			case Pickable.PickupType.E_PickupSnake:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableSnake);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupSquirrel:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableSquirrel);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupVampireBat:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableVampireBat);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupPoisonedBerry:
				m_ItemToUse = AddFireable(Fireable.FireableType.E_FireablePoisonedBerry);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupDragonCub:
				m_ItemToUse = AddFireable(Fireable.FireableType.E_FireableDragonCub);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupLavaStone:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableLavaStone);
				m_ItemUseCount = _PickupCount;
				break;
			case Pickable.PickupType.E_PickupDemonFlower:
				m_ItemToUse = AddPlaceable(Placeable.PlaceableType.E_PlaceableDemonFlower);
				m_ItemUseCount = _PickupCount;
				break;
			}

			//add render component accordingly
			
			added = (m_ItemToUse != null);
		}
		
		
		
		return added;
	}
	
	Useable AddSwingable(Swingable.SwingableType _SwingableType)
	{
		Swingable addedItem = gameObject.AddComponent<Swingable>();
		addedItem.SetSwingableType(_SwingableType);
		
		return addedItem;
	}
	
	Useable AddThrowable(Throwable.ThrowableType _ThrowableType)
	{
		Throwable addedItem = gameObject.AddComponent<Throwable>();
		addedItem.SetThrowableType(_ThrowableType);
		
		return addedItem;
	}
	
	Useable AddFireable(Fireable.FireableType _FireableType)
	{
		Fireable addedItem = gameObject.AddComponent<Fireable>();
		addedItem.SetFireableType(_FireableType);
		
		return addedItem;
	}
	
	Useable AddPlaceable(Placeable.PlaceableType _PlaceableType)
	{
		Placeable addedItem = gameObject.AddComponent<Placeable>();
		addedItem.SetPlaceableType(_PlaceableType);
		
		return addedItem;
	}
	
	void RemoveUsable()
	{		
		Destroy(m_ItemToUse);
		m_ItemToUse = null;
		m_ItemUseCount = 0;
	}
	
	void Use(Useable _ItemToUse)
	{
		_ItemToUse.Use();
		--m_ItemUseCount;
	}
}
