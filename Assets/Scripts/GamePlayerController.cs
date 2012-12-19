using UnityEngine;
using System.Collections;

public class GamePlayerController : GameCharacterController
{
	public string[] m_ActionButtons = new string[] { "Fire1", "Fire2" };
	
	public float m_SpeedMultiplier = 1.0f;
	public float m_SpeedMinimum = 0.0f;
	
	public bool m_InvertLeftRightInput = false;
	
	int m_ActionCount = 0;
	bool[] m_StartingActions;
	
	float m_Speed = 0.0f;
	float m_LeftRigthDirection = 0.0f;
	
	public override float GetSpeed()
	{
		return m_Speed;
	}
	
	public override float GetLeftRightDirection()
	{
		return m_LeftRigthDirection;
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
		m_ActionCount = m_ActionButtons.GetLength(0);
		m_StartingActions = new bool[m_ActionCount];
	}
	
	void Update()
	{
		for (int actionIndex = 0; actionIndex < m_ActionCount; ++actionIndex)
		{
			string actionButton = m_ActionButtons[actionIndex];
			bool hasActionStarted = m_StartingActions[actionIndex];
			
		    if (Input.GetButton(actionButton))
			{
				m_StartingActions[actionIndex] = true;
	        }
			else if ( hasActionStarted )
			{
				m_StartingActions[actionIndex] = false;
	        }
		}
	
		float input_h = Input.GetAxis("Horizontal");
		float input_v = Input.GetAxis("Vertical");
		
		//Debug.Log(string.Format("IdleRunJump h: {0}, v: {1}", input_h, input_v));
		
		m_Speed = Mathf.Max( m_SpeedMinimum, m_SpeedMultiplier * (input_v*input_v) );
		m_LeftRigthDirection = (m_InvertLeftRightInput)? -input_h: input_h;
	}
}
