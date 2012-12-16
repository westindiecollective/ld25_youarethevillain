using UnityEngine;
using System.Collections;

public class Throwable : Useable
{
	public enum ThrowableType { E_ThrowableNone, E_ThrowableSnake, E_ThrowableSquirrel, E_ThrowableLavaStone };
	ThrowableType m_ThrowableType = ThrowableType.E_ThrowableNone;

	float m_ProjectileVelocity = 0.0f;
	float m_ProjectileDurationInSeconds = 0.0f;
	GameObject m_Projectile = null;
	
	public void InitThrowable(ThrowableType _ThrowableType, GameObject _ProjectilePrefab, Transform _ProjectileOrigin, float _ProjectileVelocity, float _ProjectileDurationInSeconds)
	{
		m_ThrowableType = _ThrowableType;
		m_ProjectileVelocity = _ProjectileVelocity;
		m_ProjectileDurationInSeconds = _ProjectileDurationInSeconds;

		GameObject projectile = null;
		if (_ProjectilePrefab != null)
		{
			projectile = Instantiate(_ProjectilePrefab, Vector3.zero, Quaternion.identity) as GameObject;
			projectile.transform.parent = _ProjectileOrigin;

			Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
			projectileRB.isKinematic = true;
			projectileRB.useGravity = false;
		}
		else
		{
			projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			projectile.transform.parent = _ProjectileOrigin;

			projectile.transform.localPosition = Vector3.zero;
			projectile.transform.localRotation = Quaternion.identity;
			float projectileScale = 0.10f;
			projectile.transform.localScale = new Vector3(projectileScale, projectileScale, projectileScale);

			Rigidbody projectileRB = projectile.AddComponent<Rigidbody>();
			projectileRB.isKinematic = true;
			projectileRB.useGravity = false;
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
				m_Projectile.transform.parent = null;
			}

			Rigidbody projectileRB = m_Projectile.GetComponent<Rigidbody>();
			if (projectileRB != null)
			{
				projectileRB.isKinematic = false;
				projectileRB.velocity = m_ProjectileVelocity * gameObject.transform.TransformDirection( -Vector3.forward );
			}
		}
	}
}
