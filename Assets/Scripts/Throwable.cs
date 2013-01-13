using UnityEngine;
using System.Collections;

public class Throwable : Useable
{
	public enum ThrowableType { E_ThrowableNone, E_ThrowableSnake, E_ThrowableSquirrel, E_ThrowableLavaStone };
	ThrowableType m_ThrowableType = ThrowableType.E_ThrowableNone;

	GameObject m_Projectile = null;
	Vector3 m_ProjectileDirectionRelativeToGameObject = Vector3.zero;
	float m_ProjectileVelocity = 0.0f;
	float m_ProjectileDurationInSeconds = 0.0f;

	public void InitThrowable(ThrowableType _ThrowableType, GameObject _ProjectilePrefab, Transform _ProjectileOrigin, Vector3 _ProjectileDirection, float _ProjectileVelocity, float _ProjectileDurationInSeconds)
	{
		m_ThrowableType = _ThrowableType;
		m_ProjectileDirectionRelativeToGameObject = _ProjectileDirection;
		m_ProjectileVelocity = _ProjectileVelocity;
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
			float projectileScale = 0.1f;
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
		if (m_Projectile != null)
		{
			DelayedSelfDestruct selfDestruct = m_Projectile.AddComponent<DelayedSelfDestruct>();
			selfDestruct.m_DelayInSeconds = m_ProjectileDurationInSeconds;

			if (m_Projectile.transform.parent != null)
			{
				m_Projectile.transform.position = m_Projectile.transform.parent.position;
				m_Projectile.transform.rotation = Quaternion.identity;
				m_Projectile.transform.parent = null;
			}

			Rigidbody projectileRB = m_Projectile.GetComponent<Rigidbody>();
			if (projectileRB != null)
			{
				projectileRB.isKinematic = false;
				projectileRB.useGravity = false;
				projectileRB.angularVelocity = Vector3.zero;
				projectileRB.velocity = m_ProjectileVelocity * gameObject.transform.TransformDirection( m_ProjectileDirectionRelativeToGameObject );
			}

			Collider projectileCollider = m_Projectile.GetComponent<Collider>();
			if (projectileCollider != null)
			{
				projectileCollider.enabled = true;
				projectileCollider.isTrigger = true;
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
