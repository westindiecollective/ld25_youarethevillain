using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
	public int m_SwingCountPerBranch = -1;
	public float m_FireDurationPerDragonCub = 10.0f;
	public float m_ShootDurationPerPoisonBerries = 4.0f;
	public GameObject m_SnakeProjectilePrefab = null;
	public float m_SnakeProjectileVelocity = 1.0f;
	public GameObject m_SquirrelProjectilePrefab = null;
	public float m_SquirrelProjectileVelocity = 1.0f;
	public GameObject m_LavaStoneProjectilePrefab = null;
	public float m_LavaStoneProjectileVelocity = 1.0f;
	public GameObject m_DemonFlowerProjectilePrefab = null;
	public GameObject m_VampireBatProjectilePrefab = null;
	public float m_DropDurationInSeconds = 1.0f;

	public Transform m_UseableBoneOrigin = null;
	public float m_ProjectileDurationInSeconds = 1.0f;

	public bool m_DropPreviousIfNewPickup = false;

	Useable m_ItemToUse = null;
	int m_ItemUseCount = 0;

	private Texture2D m_CurrentIcon = null;
	public Texture2D m_IconEmpty, m_IconBranch, m_IconSnake, m_IconSquirrel, m_IconVampireBat, m_IconPoisonedBerry, m_IconDragonCub, m_IconLavaStone, m_IconDemonFlower;

	GameCharacterController m_GameCharacterController = null;

	void Start()
	{
		SetupGameCharacterController( GetComponent<GameCharacterController>() );
	}

	void SetupGameCharacterController(GameCharacterController _GameCharacterController)
	{
		m_GameCharacterController = _GameCharacterController;
	}

	void Update()
	{
		if (m_GameCharacterController)
		{
			if (m_ItemToUse)
			{
				List<CharacterActionType> startedActions = m_GameCharacterController.GetActions();
				foreach (CharacterActionType action in startedActions)
				{
					if (action == CharacterActionType.E_ActionUse)
					{
						Use(m_ItemToUse);
						break;
					}
				}

				if ( m_ItemUseCount == 0 )
				{
					RemoveUseable();
				}
	        }
		}
	}

	void OnGUI ()
	{
		if (m_CurrentIcon)
		{
			GUI.Box(new Rect( 40, Screen.height - 100, 80, 75), m_CurrentIcon);
		}
		else
		{
			GUI.Box(new Rect( 40, Screen.height - 100, 80, 75), m_IconEmpty);
		}
	}

	public bool AddPickup(PickupType _PickupType, int _PickupCount)
	{
		bool pickedup = AddUseableFromPickup(_PickupType, _PickupCount);

		return pickedup;
	}
	
	bool AddUseableFromPickup(PickupType _PickupType, int _PickupCount)
	{
		bool added = false;

		if ( m_DropPreviousIfNewPickup )
		{
			RemoveUseable();
		}

		if ( m_ItemToUse == null )
		{
			//add useable component accordingly
			switch (_PickupType)
			{			
			case PickupType.E_PickupBranch:
				m_ItemToUse = AddSwingable(Swingable.SwingableType.E_SwingableBranch);
				m_ItemUseCount = _PickupCount * m_SwingCountPerBranch;
				m_CurrentIcon = m_IconBranch;
				break;
			case PickupType.E_PickupSnake:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableSnake, m_SnakeProjectilePrefab, m_UseableBoneOrigin, m_SnakeProjectileVelocity, m_ProjectileDurationInSeconds);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconSnake;
				break;
			case PickupType.E_PickupSquirrel:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableSquirrel, m_SquirrelProjectilePrefab, m_UseableBoneOrigin, m_SquirrelProjectileVelocity, m_ProjectileDurationInSeconds);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconSquirrel;
				break;
			case PickupType.E_PickupVampireBat:
				m_ItemToUse = AddPlaceable(Placeable.PlaceableType.E_PlaceableVampireBat, m_VampireBatProjectilePrefab, m_UseableBoneOrigin, m_DropDurationInSeconds);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconVampireBat;
				break;
			case PickupType.E_PickupPoisonedBerry:
				m_ItemToUse = AddFireable(Fireable.FireableType.E_FireablePoisonedBerries);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconPoisonedBerry;
				break;
			case PickupType.E_PickupDragonCub:
				m_ItemToUse = AddFireable(Fireable.FireableType.E_FireableDragonCub);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconDragonCub;
				break;
			case PickupType.E_PickupLavaStone:
				m_ItemToUse = AddThrowable(Throwable.ThrowableType.E_ThrowableLavaStone, m_LavaStoneProjectilePrefab, m_UseableBoneOrigin, m_LavaStoneProjectileVelocity, m_ProjectileDurationInSeconds);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconLavaStone;
				break;
			case PickupType.E_PickupDemonFlower:
				m_ItemToUse = AddPlaceable(Placeable.PlaceableType.E_PlaceableDemonFlower, m_DemonFlowerProjectilePrefab, m_UseableBoneOrigin, m_DropDurationInSeconds);
				m_ItemUseCount = _PickupCount;
				m_CurrentIcon = m_IconDemonFlower;
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

	Useable AddThrowable(Throwable.ThrowableType _ThrowableType, GameObject _ProjectilePrefab, Transform _ProjectileOrigin, float _ProjectileVelocity, float _ProjectileDurationInSeconds)
	{
		Throwable addedItem = gameObject.AddComponent<Throwable>();
		//Throwing behind the character
		Vector3 throwDirection = -Vector3.forward;

		addedItem.InitThrowable(_ThrowableType, _ProjectilePrefab, _ProjectileOrigin, throwDirection, _ProjectileVelocity, _ProjectileDurationInSeconds);
		
		return addedItem;
	}

	Useable AddFireable(Fireable.FireableType _FireableType)
	{
		Fireable addedItem = gameObject.AddComponent<Fireable>();
		addedItem.SetFireableType(_FireableType);

		return addedItem;
	}

	Useable AddPlaceable(Placeable.PlaceableType _PlaceableType, GameObject _ProjectilePrefab, Transform _ProjectileOrigin, float _ProjectileDurationInSeconds)
	{
		Placeable addedItem = gameObject.AddComponent<Placeable>();
		addedItem.InitPlaceable(_PlaceableType, _ProjectilePrefab, _ProjectileOrigin, _ProjectileDurationInSeconds);

		return addedItem;
	}

	void RemoveUseable()
	{
		m_ItemToUse = null;
		m_ItemUseCount = 0;

		m_CurrentIcon = null;
    }

	void Use(Useable _ItemToUse)
	{
		_ItemToUse.Use();
		--m_ItemUseCount;
	}
}
