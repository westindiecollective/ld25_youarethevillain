#define DEBUG_AIFOLLOW_CONTROLLER

using UnityEngine;
using System.Collections;

public class GameAIFollowController : GameCharacterController
{
	public GameObject m_FollowTarget = null;
	public float m_FollowSpeed = 0.5f;
	
	public int m_UpdatePathFrameFrequency = 1;
	int m_FrameCountSinceLastPath = 0;
	NavMeshAgent m_NavAgent = null;
	
	CharacterController m_CharacterController = null;
	
#if DEBUG_AIFOLLOW_CONTROLLER
	public GameObject m_DebugDestination = null;
#endif
	
	public override float GetInputSpeed()
	{
		return m_NavAgent? m_FollowSpeed : 0.0f;
	}
	
	public override float GetInputLeftRightDirection()
	{
		Vector3 desiredForwardDir = m_NavAgent? Vector3.Normalize(m_NavAgent.desiredVelocity) : Vector3.zero;
		Vector3 rightDir = gameObject.transform.right;
		float leftRightDir = Vector3.Dot(desiredForwardDir, rightDir);
		
		return leftRightDir;
	}
	
	public override Vector3 GetPosition()
	{
		return gameObject.transform.position;
	}
	
	public override void SetPosition(Vector3 _Position)
	{
		gameObject.transform.position = _Position;
	}
	
	public override Quaternion GetOrientation()
	{
		return gameObject.transform.rotation;
	}
	
	public override void SetOrientation(Quaternion _Orientation)
	{
		gameObject.transform.rotation = _Orientation;
	}
	
	public override float GetVelocity()
	{
		return m_CharacterController.velocity.magnitude;
	}
	
	public override bool IsStartingAction(int _ActionIndex)
	{
		return false;
	}

	void Start()
	{
		m_CharacterController = GetComponent<CharacterController>();
		
		m_NavAgent = GetComponent<NavMeshAgent>();
		m_NavAgent.updatePosition = false;
		m_NavAgent.updateRotation = false;
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_NavAgent && m_FollowTarget)
		{
			++m_FrameCountSinceLastPath;
			bool updatePath = (m_FrameCountSinceLastPath >= m_UpdatePathFrameFrequency) && CanUpdatePath();
			if (updatePath)
			{
				UpdatePath(deltaTime, m_FollowTarget.gameObject.transform.position);
				m_FrameCountSinceLastPath = 0;
			}
		}
	}
	
	void UpdatePath(float _DeltaTime, Vector3 _Destination)
	{
		m_NavAgent.SetDestination(_Destination);
		
#if DEBUG_AIFOLLOW_CONTROLLER	
		if (m_DebugDestination)
			m_DebugDestination.transform.position = _Destination;
#endif
	}
	
	bool CanUpdatePath()
	{
		return !m_NavAgent.pathPending;
	}
}
