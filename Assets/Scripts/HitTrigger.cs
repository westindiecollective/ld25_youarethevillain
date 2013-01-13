using UnityEngine;
using System.Collections;

public class HitTrigger : MonoBehaviour
{
	//Hit impact wont trigger anything when colliding with owner as it can happen upon firing the projectile
	GameObject m_Owner = null;

	public void Init(GameObject _Owner)
	{
		m_Owner = _Owner;
	}

	void Start()
	{
	}

	void Update()
	{
	}

	void OnTriggerEnter(Collider other)
	{
		if ( other.gameObject == m_Owner )
		{
			//discard the collision
		}
		else
		{
			Debug.Log(string.Format("Game object {0} has been hit with projectile.", other.gameObject.name));

			HitReceiver hitHandler = other.gameObject.GetComponent<HitReceiver>();
			if (hitHandler)
			{
				hitHandler.HandleHit();
			}

			//TODO: hide render component, spawn particles, ...

			DestroyObject(gameObject);
		}
	}
}
