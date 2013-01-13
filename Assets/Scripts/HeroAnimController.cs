using UnityEngine;
using System.Collections;

public class HeroAnimController : CharacterAnimController
{
	Animator m_Animator = null;
	GameCharacterController m_GameCharacterController = null;

	public int m_JumpActionIndex = 0;
	public int m_ThrowActionIndex = 1;

	//@TODO: move this to GamePlayerController
	public float m_DirectionDampTime = .25f;

	public bool m_AffectSpeed = true;
	public bool m_AffectDirection = true;

	int m_HitId = 0;
	int m_JumpId = 0;
	int m_ThrowId = 0;
	int m_SpeedId = 0;
	int m_DirectionId = 0;

	bool m_TakeHit = false;

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

		m_HitId = Animator.StringToHash("Hit");
		m_JumpId = Animator.StringToHash("Jump");
		m_ThrowId = Animator.StringToHash("Throw");
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
			if (m_TakeHit)
			{
				m_Animator.SetBool(m_HitId, true);
				m_Animator.SetBool(m_JumpId, false);
				m_Animator.SetBool(m_ThrowId, false);

				m_TakeHit = false;
			}
			else
			{
				m_Animator.SetBool(m_HitId, false);

				bool startJump = m_GameCharacterController.IsStartingAction(m_JumpActionIndex);
				m_Animator.SetBool(m_JumpId, startJump);

				bool startThrow = m_GameCharacterController.IsStartingAction(m_ThrowActionIndex);
				m_Animator.SetBool(m_ThrowId, startThrow);
			}

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

			if (!gameObject.CompareTag("Player"))
			{
				AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
				if (stateInfo.IsName("Hit"))
					Debug.Log( string.Format("Hero anim state is 'Hit'") );
				else if (stateInfo.IsName("Idle"))
					Debug.Log( string.Format("Hero anim state is 'Idle'") );
			}
		}
	}

	public override void HandleHit()
	{
		m_TakeHit = true;
	}

	public override bool IsJumping()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isJumping = stateInfo.IsName("Base Layer.Jump");

		return isJumping;
	}

	public override bool IsRunning()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isRunning = stateInfo.IsName("Base Layer.Run");

		return isRunning;
	}

	public override bool IsThrowing()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isThrowing = stateInfo.IsName("Base Layer.Throw");
		
		return isThrowing;
	}

}
