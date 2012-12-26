using UnityEngine;
using System.Collections;

public class IdleRunJump : MonoBehaviour
{
	Animator m_Animator = null;
	GameCharacterController m_GameCharacterController = null;

	public int m_JumpActionIndex = 0;
	public int m_SayHiActionIndex = 1;

	//@TODO: move this to GamePlayerController
	public float m_DirectionDampTime = .25f;

	public bool m_AffectSpeed = true;
	public bool m_AffectDirection = true;

	int m_JumpId = 0;
	int m_SayHiId = 0;
	int m_SpeedId = 0;
	int m_DirectionId = 0;

	void Start ()
	{
		SetupAnimator( GetComponent<Animator>() );
		SetupGameCharacterController( GetComponent<GameCharacterController>() );
	}

	void SetupAnimator(Animator _Animator)
	{
		m_Animator = _Animator;

		if(m_Animator.layerCount >= 2)
			m_Animator.SetLayerWeight(1, 1);

		m_JumpId = Animator.StringToHash("Jump");
		m_SayHiId = Animator.StringToHash("Hi");
		m_SpeedId = Animator.StringToHash("Speed");
		m_DirectionId = Animator.StringToHash("Direction");
	}
	
	void SetupGameCharacterController(GameCharacterController _GameCharacterController)
	{
		m_GameCharacterController = _GameCharacterController;
	}

	void Update () 
	{
		float deltaTime = Time.deltaTime;

		if (m_Animator && m_GameCharacterController)
		{
			bool startJump = m_GameCharacterController.IsStartingAction(m_JumpActionIndex);
			m_Animator.SetBool(m_JumpId, startJump);

			bool startSayHi = m_GameCharacterController.IsStartingAction(m_SayHiActionIndex);
			m_Animator.SetBool(m_SayHiId, startSayHi);
			
			if (m_AffectSpeed)
			{
				float speed = m_GameCharacterController.GetInputSpeed();
				m_Animator.SetFloat(m_SpeedId, speed);
			}
			
			if (m_AffectDirection)
			{
				float leftRightDirection = m_GameCharacterController.GetInputLeftRightDirection();
				float direction = Mathf.Clamp01(leftRightDirection);
				m_Animator.SetFloat(m_DirectionId, direction, m_DirectionDampTime, deltaTime);
			}
		}
	}

	public bool IsJumping()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isJumping = stateInfo.IsName("Base Layer.Jump");

		return isJumping;
	}
}
