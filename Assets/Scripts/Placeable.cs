using UnityEngine;
using System.Collections;

public class Placeable : Useable
{
	public enum PlaceableType { E_PlaceableNone, E_PlaceableVampireBat, E_PlaceableDemonFlower };
	PlaceableType m_PlaceableType;

	float m_ProjectileDurationInSeconds = 0.0f;
	GameObject m_Projectile = null;

	public void InitPlaceable(PlaceableType _PlaceableType, GameObject _ProjectilePrefab, Transform _ProjectileOrigin, float _ProjectileDurationInSeconds)
	{
		m_PlaceableType = _PlaceableType;
		m_ProjectileDurationInSeconds = _ProjectileDurationInSeconds;

		GameObject projectile = null;
		if (_ProjectilePrefab != null)
		{
			projectile = Instantiate(_ProjectilePrefab, Vector3.zero, Quaternion.identity) as GameObject;
			projectile.transform.parent = _ProjectileOrigin;
		}
		else
		{
			//DEBUG
			projectile = GameObject.CreatePrimitive(PrimitiveType.Cube);

			projectile.transform.parent = _ProjectileOrigin;
			projectile.transform.localPosition = Vector3.zero;
			projectile.transform.localRotation = Quaternion.identity;
			float projectileScale = 0.40f;
			projectile.transform.localScale = new Vector3(projectileScale, projectileScale, projectileScale);

			projectile.AddComponent<Rigidbody>();

			projectile.AddComponent<HitTrigger>();
		}

		Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
		if (projectileRB)
		{
			projectileRB.isKinematic = true;
		}

		Collider projectileCollider = projectile.GetComponent<Collider>();
		if (projectileCollider != null)
		{
			projectileCollider.enabled = false;
		}

		HitTrigger projectileHitTrigger = projectile.GetComponent<HitTrigger>();
		if (projectileHitTrigger != null)
		{
			projectileHitTrigger.enabled = false;
		}

		m_Projectile = projectile;
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
		if (m_Projectile != null)
		{
			DelayedSelfDestruct selfDestruct = m_Projectile.AddComponent<DelayedSelfDestruct>();
			selfDestruct.m_DelayInSeconds = m_ProjectileDurationInSeconds;

			if (m_Projectile.transform.parent != null)
			{
				m_Projectile.transform.position = m_Projectile.transform.parent.position;
				m_Projectile.transform.parent = null;
			}

			Rigidbody projectileRB = m_Projectile.GetComponent<Rigidbody>();
			if (projectileRB != null)
			{
				projectileRB.isKinematic = false;
				projectileRB.useGravity = true;
			}

			Collider projectileCollider = m_Projectile.GetComponent<Collider>();
			if (projectileCollider != null)
			{
				projectileCollider.enabled = true;
			}

			HitTrigger projectileHitTrigger = m_Projectile.GetComponent<HitTrigger>();
			if (projectileHitTrigger != null)
			{
				projectileHitTrigger.enabled = true;
				projectileHitTrigger.Init(gameObject);
			}
		}
	}
}
