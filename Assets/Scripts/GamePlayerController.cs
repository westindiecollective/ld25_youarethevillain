using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayerController : GameCharacterController
{
	public CharacterAction[] m_SupportedActions;

	LinkedList<CharacterAction> m_ActiveActions = null;
	List<CharacterActionType> m_Actions = null;
	bool m_CanStartAction = false;
	bool m_HandleHit = false;

	public float m_SpeedMultiplier = 1.0f;
	public float m_SpeedMinimum = 0.0f;

	public bool m_InvertLeftRightInput = false;

	float m_InputSpeed = 0.0f;
	float m_InputLeftRigthDirection = 0.0f;

	CharacterController m_CharacterController = null;

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

	public override List<CharacterActionType> GetActions()
	{
		return m_Actions;
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
	}
	
	void Update()
	{
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
