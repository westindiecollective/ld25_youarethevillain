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
			//AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);			

			bool startJump = m_GameCharacterController.IsStartingAction(m_JumpActionIndex);
			m_Animator.SetBool("Jump", startJump);

			bool startSayHi = m_GameCharacterController.IsStartingAction(m_SayHiActionIndex);
			m_Animator.SetBool("Hi", startSayHi);
			
			if (m_AffectSpeed)
			{
				float speed = m_GameCharacterController.GetSpeed();
				m_Animator.SetFloat("Speed", speed);
			}
			
			if (m_AffectDirection)
			{
				float leftRightDirection = m_GameCharacterController.GetLeftRightDirection();
				float direction = Mathf.Clamp01(leftRightDirection);
				m_Animator.SetFloat("Direction", direction, m_DirectionDampTime, deltaTime);
			}
		}   		  
	}
}
