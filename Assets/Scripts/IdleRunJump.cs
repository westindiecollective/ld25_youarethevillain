using UnityEngine;
using System.Collections;

public class IdleRunJump : MonoBehaviour
{
	Animator m_Animator;

	void SetupAnimator(Animator _Animator)
	{
		m_Animator = _Animator;

		if(m_Animator.layerCount >= 2)
			m_Animator.SetLayerWeight(1, 1);
	}

	public float m_DirectionDampTime = .25f;
	public float m_SpeedFactor = 1.0f;
	public float m_SpeedAuto = 0.0f;
	
	bool m_StartedJump = false;
	bool m_StartedSayingHi = false;
	
	public bool m_AffectSpeed = true;
	public bool m_AffectDirection = true;

	void Start () 
	{
		Animator animator = GetComponent<Animator>();
		SetupAnimator(animator);
	}

	void Update () 
	{
		if (m_Animator)
		{
			//AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);			

			if (Input.GetButton("Fire1"))
			{
				m_Animator.SetBool("Jump", true);
				m_StartedJump = true;
            }
			else if ( m_StartedJump )
			{
				m_Animator.SetBool("Jump", false);
				m_StartedJump = false;
            }

			if (Input.GetButtonDown("Fire2") && m_Animator.layerCount >= 2)
			{
				m_Animator.SetBool("Hi", true);
				m_StartedSayingHi = true;
			}
			else if (m_StartedSayingHi)
			{
				m_Animator.SetBool("Hi", false);
				m_StartedSayingHi = false;
			}
		
			float v = Mathf.Max( m_SpeedAuto, Input.GetAxis("Vertical") );
			float h = Input.GetAxis("Horizontal");

			//Debug.Log(string.Format("IdleRunJump h: {0}, v: {1}", h, v));
			
			if (m_AffectSpeed)
			{
				float speed = m_SpeedFactor * (h*h+v*v);
				m_Animator.SetFloat("Speed", speed);
			}
			
			if (m_AffectDirection)
			{
				m_Animator.SetFloat("Direction", h, m_DirectionDampTime, Time.deltaTime);
			}
		}   		  
	}
}
