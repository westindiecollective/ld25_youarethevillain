using UnityEngine;
using System.Collections;

public class GamePlayerController : GameCharacterController
{
	public string[] m_ActionButtons = new string[] { "Fire1", "Fire2" };

	public float m_SpeedMultiplier = 1.0f;
	public float m_SpeedMinimum = 0.0f;

	public bool m_InvertLeftRightInput = false;

	int m_ActionCount = 0;
    bool m_CanStartAction = false;
	bool[] m_StartingActions;

	float m_InputSpeed = 0.0f;
	float m_InputLeftRigthDirection = 0.0f;

	CharacterController m_CharacterController = null;
	AnimationController m_AnimationController = null;

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

	public override bool IsStartingAction(int _ActionIndex)
	{
		bool isValidActionIndex = (0 <= _ActionIndex && _ActionIndex < m_ActionCount);
		System.Diagnostics.Debug.Assert(isValidActionIndex);
		
		bool isStartingAction = isValidActionIndex? m_StartingActions[_ActionIndex] : false;
		return isStartingAction;
	}

	void Start()
	{
		EnableActions();

		m_ActionCount = m_ActionButtons.GetLength(0);
		m_StartingActions = new bool[m_ActionCount];

		m_CharacterController = GetComponent<CharacterController>();
		m_AnimationController = GetComponent<AnimationController>();
	}
	
	void Update()
	{
		for (int actionIndex = 0; actionIndex < m_ActionCount; ++actionIndex)
		{
			bool hasActionStarted = m_StartingActions[actionIndex];
			if ( hasActionStarted )
			{
				m_StartingActions[actionIndex] = false;
			}
		}

		if ( m_CanStartAction )
		{
			for (int actionIndex = 0; actionIndex < m_ActionCount; ++actionIndex)
			{
				string actionButton = m_ActionButtons[actionIndex];
				if (Input.GetButton(actionButton))
				{
					m_StartingActions[actionIndex] = true;
				}
			}
		}

		float input_h = Input.GetAxis("Horizontal");
		float input_v = Input.GetAxis("Vertical");

		//Debug.Log(string.Format("IdleRunJump h: {0}, v: {1}", input_h, input_v));

		m_InputSpeed = Mathf.Max( m_SpeedMinimum, m_SpeedMultiplier * (input_v*input_v) );
		m_InputLeftRigthDirection = (m_InvertLeftRightInput)? -input_h: input_h;
	}

	public override void HandleHit()
	{
		if (m_AnimationController)
		{
			m_AnimationController.HandleHit();
		}
	}

}
