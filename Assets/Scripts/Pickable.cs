using UnityEngine;
using System.Collections;

public class Pickable : MonoBehaviour
{
	public enum PickupType { E_PickupNone,
		E_PickupBranch,
		E_PickupSnake,
		E_PickupSquirrel,
		E_PickupVampireBat,
		E_PickupPoisonedBerry,
		E_PickupDragonCub,
		E_PickupLavaStone,
		E_PickupDemonFlower
	};
	public PickupType m_PickupType = PickupType.E_PickupNone;
	public int m_PickupCount = 1;
	
	//add render component accordingly
	//add trigger component accordingly
	
	void Start()
	{
		
	}
	
	void Update()
	{
	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if ( other.CompareTag("Player") )
		{
			Inventory inventory = other.gameObject.GetComponent<Inventory>();
			if (inventory)
			{
				bool pickedup = inventory.AddPickup(m_PickupType, m_PickupCount);
				if ( pickedup )
				{
					DestroyObject(gameObject);	
				}
			}
		}
	}
}
