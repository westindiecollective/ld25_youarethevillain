#define DEBUG_AIFOLLOW_CONTROLLER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameAIFollowController : GameCharacterController
{
	public GameObject m_FollowTarget = null;
	public float m_FollowSpeed = 0.5f;

	public int m_UpdatePathFrameFrequency = 1;
	int m_FrameCountSinceLastPath = 0;
	NavMeshAgent m_NavAgent = null;

	//FIXME: duplicated code from GamePlayerController
	public CharacterAction[] m_SupportedActions;
	LinkedList<CharacterAction> m_ActiveActions = null;
	List<CharacterActionType> m_Actions = null;
	bool m_CanStartAction = false;
	bool m_HandleHit = false;

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

	//FIXME: duplicated code from GamePlayerController
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

	void Start()
	{
		InitActions();
		EnableActions();

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

			//FIXME: duplicated code from GamePlayerController
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
