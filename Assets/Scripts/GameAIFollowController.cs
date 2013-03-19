#define DEBUG_AIFOLLOW_CONTROLLER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameAIFollowController : GameCharacterController
{
	public GameObject m_FollowTarget = null;
	public float m_FollowSpeed = 0.5f;
	Vector3 m_FollowRightDir = Vector3.zero;

	public int m_UpdatePathFrameFrequency = 1;
	int m_FrameCountSinceLastPath = 0;
	NavMeshAgent m_NavAgent = null;

	//@FIXME: duplicated code from GamePlayerController
	public CharacterAction[] m_SupportedActions;
	LinkedList<CharacterAction> m_ActiveActions = null;
	List<CharacterActionType> m_Actions = null;
	bool m_CanStartAction = false;
	bool m_HandleHit = false;
	
	bool m_CanUpdateCollisionCenter = false;

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
		Vector3 desiredForwardDir = (m_NavAgent && m_NavAgent.hasPath)? Vector3.Normalize(m_NavAgent.path.corners[1] - gameObject.transform.position) : Vector3.zero;
		Vector3 rightDir = m_FollowRightDir;
		float leftRightDir = Vector3.Dot(desiredForwardDir, rightDir);

		return leftRightDir;
	}

	void UpdateFollowDir( Vector3 _followUpDir, Vector3 _followForwardDir)
	{
		Vector3 upDir = _followUpDir;
		Vector3 forwardDir = _followForwardDir;
		Vector3 rightDir = Vector3.Cross(upDir, forwardDir);

		m_FollowRightDir = rightDir;
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
	
	public override bool CanUpdateCollisionCenter()
	{
		return m_CanUpdateCollisionCenter;
	}
	
	public override void AuthorizeUpdatingCollisionCenter()
	{
		m_CanUpdateCollisionCenter = true;
	}
	
	public override void UnauthorizeUpdatingCollisionCenter()
	{
		m_CanUpdateCollisionCenter = false;
	}
	
	public override void UpdateCollisionHeight(float _Height)
	{
		m_CharacterController.height = _Height;
	}
	
	public override void UpdateCollisionCenter(Vector3 _Center)
	{
		m_CharacterController.center = _Center;
	}

	public override void EnableActions()
	{
		m_CanStartAction = true;
	}

	public override void DisableActions()
	{
		m_CanStartAction = false;
	}

	public override void EnableAction(CharacterActionType _Action)
	{
		int actionCount = m_SupportedActions.Length;
		for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
		{
			CharacterAction action = m_SupportedActions[actionIndex];
			if (action.m_ActionType == _Action)
			{
				m_ActiveActions.AddFirst(action);
				break;
			}
		}
	}

	public override void DisableAction(CharacterActionType _Action)
	{
		foreach (CharacterAction action in m_ActiveActions)
		{
			if (action.m_ActionType == _Action)
			{
				m_ActiveActions.Remove(action);
				break;
			}
		}
	}

	//@FIXME: duplicated code from GamePlayerController
	public override List<CharacterActionType> GetActions()
	{
		return m_Actions;	//new List<CharacterActionType>();
	}

	void InitActions()
	{
		m_ActiveActions = new LinkedList<CharacterAction>();
		m_Actions = new List<CharacterActionType>();
	}

	void StartAction(CharacterActionType _ActionType)
	{
		m_Actions.Add(_ActionType);
	}

	void ClearActions()
	{
		m_Actions.Clear();
	}

	public override void TriggerPendingAction(CharacterActionType _Action)
	{
	}

	void Start()
	{
		InitActions();
		EnableActions();

		m_CharacterController = GetComponent<CharacterController>();

		m_NavAgent = GetComponent<NavMeshAgent>();
		m_NavAgent.updatePosition = false;
		m_NavAgent.updateRotation = false;

		//HACK: up and forward dir should be provided by sthg and not hardcoded
		UpdateFollowDir(Vector3.up, Vector3.forward);
	}

	void Update()
	{
		float deltaTime = Time.deltaTime;

		if (m_NavAgent && m_FollowTarget)
		{
#if DEBUG_AIFOLLOW_CONTROLLER
			NavMeshPathStatus pathStatus = m_NavAgent.pathStatus;
			if (pathStatus != NavMeshPathStatus.PathInvalid)
			{
				Color pathColor = (pathStatus == NavMeshPathStatus.PathComplete)? Color.green : Color.blue;

				NavMeshPath path = m_NavAgent.path;
				int pathElements = path.corners.Length;
				for (int i=1; i<pathElements; ++i)
				{
				    Debug.DrawLine(path.corners[i-1], path.corners[i], pathColor);
				}
			}
#endif
			++m_FrameCountSinceLastPath;
			bool updatePath = (m_FrameCountSinceLastPath >= m_UpdatePathFrameFrequency) && CanUpdatePath();
			if (updatePath)
			{
				UpdatePath(deltaTime, m_FollowTarget.gameObject.transform.position);
				m_FrameCountSinceLastPath = 0;
			}

			//@FIXME: duplicated code from GamePlayerController
			ClearActions();

			if (m_CanStartAction)
			{
				foreach (CharacterAction action in m_ActiveActions)
				{
					string actionButton = action.m_ActionButton;
					if (actionButton.Length > 0 && Input.GetButton(actionButton))
					{
						StartAction(action.m_ActionType);
					}
				}
			}

			if (m_HandleHit)
			{
				//check whether hit action is supported
				int actionCount = m_SupportedActions.Length;
				for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
				{
					CharacterAction action = m_SupportedActions[actionIndex];
					if (action.m_ActionType == CharacterActionType.E_ActionTakeHit)
					{
						StartAction(CharacterActionType.E_ActionTakeHit);
						break;
					}
				}
				m_HandleHit = false;
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

	public override void HandleHit()
	{
		m_HandleHit = true;
	}
}
