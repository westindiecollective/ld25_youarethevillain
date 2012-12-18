using UnityEngine;
using System.Collections;

public class ScalePlayerCollision : MonoBehaviour
{

	private Animator m_Animator;
	private CharacterController m_CharacterController;
	private float m_BaseCapsuleHeight;
//	private float m_BaseCapsuleCenterY;0
	private bool m_IsJumping;
	
	public float m_JumpCapsuleScaleFactor = 0.0f;
	public float m_JumpCapsuleOffsetY = 0.0f;
	
	void Start()
	{
		m_Animator = GetComponent<Animator>();
		m_CharacterController = GetComponent<CharacterController>();
		m_BaseCapsuleHeight = m_CharacterController.height;
//		m_BaseCapsuleCenterY = m_CharacterController.center.y;
//		m_IsJumping = false;
	}
	
	void Update()
	{
		if (m_Animator)
		{
/*			
			AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
			bool stateIsJump = state.IsName("Base Layer.Jump");
		
			if (stateIsJump && !m_IsJumping)
			{
				m_CharacterController.height = m_BaseCapsuleHeight * m_JumpCapsuleScaleFactor;
				
				Vector3 centerMove = new Vector3(0.0f, m_JumpCapsuleOffsetY, 0.0f);
				m_CharacterController.Move( centerMove );
				
				m_IsJumping = true;
			}
			else if (m_IsJumping && !stateIsJump)
			{
				m_IsJumping = false;
				m_CharacterController.height = m_BaseCapsuleHeight;
				
				//WARNING: Probably lack of floating point accuracy will introduce an increasing error from the base value
				Vector3 centerRestoreMove = new Vector3(0.0f, -m_JumpCapsuleOffsetY, 0.0f);
				m_CharacterController.Move( centerRestoreMove );
			}
*/			
			m_CharacterController.height = m_BaseCapsuleHeight * (1 + m_JumpCapsuleScaleFactor * m_Animator.GetFloat("JumpingCurve"));
			
		}
	}
}
