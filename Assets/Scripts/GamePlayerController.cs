#define DEBUG_PLAY_GAME_IN_SLOW_MOTION

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayerController : GameCharacterController
{
	public CharacterAction[] m_SupportedActions;

	LinkedList<CharacterAction> m_ActiveActions = null;
	List<CharacterActionType> m_Actions = null;
	List<CharacterActionType> m_PendingActions = null;
	List<CharacterActionType> m_PendingActionsToTrigger = null;
	bool m_CanStartAction = false;
	bool m_HandleHit = false;
	bool m_CanUpdateCollisionCenter = false;
	
	Texture2D m_CurrentActionIcon = null;

	public float m_SpeedMultiplier = 1.0f;
	public float m_SpeedMinimum = 0.0f;

	public bool m_InvertLeftRightInput = false;

	float m_InputSpeed = 0.0f;
	float m_InputLeftRigthDirection = 0.0f;

	CharacterController m_CharacterController = null;

#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
	public string m_SlowMotionInputButton = "";
	public float m_SlowMotionTimeScale = 0.5f;
	float m_FixedDeltaTimeRatio = 0.0f;
	bool m_PlayInSlowMotion = false;
#endif

	public override float GetInputSpeed()
	{
		return m_InputSpeed;
	}

	public override float GetInputLeftRightDirection()
	{
		return m_InputLeftRigthDirection;
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
		return m_CharacterController? m_CharacterController.velocity.magnitude : 0.0f;
	}
	
	//NOTE: updating character collision center will reset Unity internal data for OnTriggerEnter() events
	//causing multiple successive enter events being fired
	//To avoid this, when entering a trigger volume, UnauthorizeUpdatingCollisionCenter() should be called
	//and when exiting the trigger volume, AuthorizeUpdatingCollisionCenter() should be called
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
				m_CurrentActionIcon = action.m_ActionIcon;
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
				m_CurrentActionIcon = null;
				break;
			}
		}
	}

	public override List<CharacterActionType> GetActions()
	{
		return m_Actions;
	}

	void InitActions()
	{
		m_ActiveActions = new LinkedList<CharacterAction>();
		m_Actions = new List<CharacterActionType>();
		m_PendingActions = new List<CharacterActionType>();
		m_PendingActionsToTrigger = new List<CharacterActionType>();
	}

	void StartAction(CharacterActionType _ActionType)
	{
		m_Actions.Add(_ActionType);
	}

	void ClearActions()
	{
		m_Actions.Clear();
	}
	
	public override void TriggerPendingAction(CharacterActionType _ActionType)
	{
		m_PendingActionsToTrigger.Add(_ActionType);
	}
	
	void ClearPendingActionsToTrigger()
	{
		m_PendingActionsToTrigger.Clear();
	}
	
	bool IsActionPending(CharacterActionType _ActionType)
	{
		bool isActionPending = m_PendingActions.Contains(_ActionType);
		return isActionPending;
	}
	
	void AddPendingAction(CharacterActionType _ActionType)
	{
		m_PendingActions.Add(_ActionType);
	}
	
	void StartPendingAction(CharacterActionType _ActionType)
	{
		foreach (CharacterActionType pendingActionType in m_PendingActions)
		{
			if (pendingActionType == _ActionType)
			{
				StartAction(_ActionType);

				m_PendingActions.Remove(_ActionType);
				
				break;
			}
		}
	}
	
	void ClearPendingActions()
	{
		m_PendingActions.Clear();
	}

	void Start()
	{
		InitActions();
		EnableActions();

		m_CharacterController = GetComponent<CharacterController>();
		
		AuthorizeUpdatingCollisionCenter();

#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
		m_FixedDeltaTimeRatio = Time.fixedDeltaTime / Time.timeScale;
		if (m_PlayInSlowMotion)
		{
			Time.timeScale = m_PlayInSlowMotion? m_SlowMotionTimeScale : 1.0f;
			Time.fixedDeltaTime = m_FixedDeltaTimeRatio * Time.timeScale;
		}
#endif
	}
	
	void OnGUI ()
	{
		if (m_CurrentActionIcon)
		{
			GUI.Box(new Rect(Screen.width / 2 - 40, 100, 80, 75), m_CurrentActionIcon);
		}
	}
	
	void Update()
	{
#if DEBUG_PLAY_GAME_IN_SLOW_MOTION
		bool switchSlowMotionMode = (m_SlowMotionInputButton.Length > 0) && Input.GetButtonUp(m_SlowMotionInputButton);
		if (switchSlowMotionMode)
		{
			m_PlayInSlowMotion = !m_PlayInSlowMotion;
			Time.timeScale = m_PlayInSlowMotion? m_SlowMotionTimeScale : 1.0f;
			Time.fixedDeltaTime = m_FixedDeltaTimeRatio * Time.timeScale;
		}
#endif

		ClearActions();

		if (m_CanStartAction)
		{
			foreach (CharacterAction action in m_ActiveActions)
			{
				string actionButton = action.m_ActionButton;
				if (actionButton.Length > 0 && Input.GetButtonUp(actionButton))
				{
					bool startAction = action.m_ActionIsImmediate;
					if (startAction)
					{
						StartAction(action.m_ActionType);
					}
					else if ( IsActionPending(action.m_ActionType) == false )
					{
						AddPendingAction(action.m_ActionType);
					}
				}
			}
			
			foreach (CharacterActionType actionType in m_PendingActionsToTrigger)
			{
				StartPendingAction(actionType);
			}
			
			ClearPendingActionsToTrigger();
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

		float input_h = Input.GetAxis("Horizontal");
		float input_v = Input.GetAxis("Vertical");

		//Debug.Log(string.Format("IdleRunJump h: {0}, v: {1}", input_h, input_v));

		m_InputSpeed = Mathf.Max( m_SpeedMinimum, m_SpeedMultiplier * (input_v*input_v) );
		m_InputLeftRigthDirection = (m_InvertLeftRightInput)? -input_h: input_h;
	}

	public override void HandleHit()
	{
		m_HandleHit = true;
	}

}
