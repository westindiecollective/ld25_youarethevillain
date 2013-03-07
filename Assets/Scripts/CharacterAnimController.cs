using UnityEngine;
using System.Collections;

public abstract class CharacterAnimController : AnimationController
{
	[System.Serializable]
	public class CharacterAnimAction
	{
		public CharacterActionType m_ActionType = CharacterActionType.E_ActionNone;
		int m_AnimParamId;
		public string m_AnimParam;

		public void InitParamId()
		{
			m_AnimParamId = Animator.StringToHash(m_AnimParam);
		}

		public int GetParamId()
		{
			return m_AnimParamId;
		}
	}

	public abstract bool IsJumping();

	public abstract bool IsRunning();

	public abstract bool IsThrowing();
	
	public abstract bool CanOrientationBeModified();
}
