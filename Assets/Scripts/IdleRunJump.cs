using UnityEngine;
using System.Collections;

public class IdleRunJump : MonoBehaviour {


	protected Animator m_Animator;
	public float m_DirectionDampTime = .25f;
	public bool m_ApplyGravity = true;
	public float m_SpeedFactor = 1.0f;
	public float m_SpeedAuto = 0.5f;

	// Use this for initialization
	void Start () 
	{
		m_Animator = GetComponent<Animator>();
		
		if(m_Animator.layerCount >= 2)
			m_Animator.SetLayerWeight(1, 1);
	}
		
	// Update is called once per frame
	void Update () 
	{
		if (m_Animator)
		{
			AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);			

			if (stateInfo.IsName("Base Layer.Run"))
			{
				if (Input.GetButton("Fire1")) m_Animator.SetBool("Jump", true);                
            }
			else
			{
				m_Animator.SetBool("Jump", false);                
            }

			if(Input.GetButtonDown("Fire2") && m_Animator.layerCount >= 2)
			{
				m_Animator.SetBool("Hi", !m_Animator.GetBool("Hi"));
			}
			
		
      		float h = Input.GetAxis("Horizontal");
        	float v = Mathf.Max( m_SpeedAuto, Input.GetAxis("Vertical") );
			
			//Debug.Log(string.Format("h: {0}, v: {1}", h, v));
			
			float speed = m_SpeedFactor * (h*h+v*v);
			m_Animator.SetFloat("Speed", speed);
            m_Animator.SetFloat("Direction", h, m_DirectionDampTime, Time.deltaTime);	
		}   		  
	}
}
