using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VillainAnimController : CharacterAnimController
{
	public CharacterAnimAction[] m_SupportedAnimActions;

	Animator m_Animator = null;
	GameCharacterController m_GameCharacterController = null;

	//@TODO: move this to GamePlayerController
	public float m_DirectionDampTime = .25f;

	public bool m_AffectSpeed = true;
	public bool m_AffectDirection = true;

	int m_SpeedId = 0;
	int m_DirectionId = 0;
	
	private float m_BaseCapsuleHeight;
	private Vector3 m_BaseCapsuleCenter;
	public float m_JumpCapsuleScaleFactor = 0.0f;
	public float m_JumpCapsuleOffsetY = 0.0f;
	int m_JumpingCurveId = 0;

	int FindAnimParamId( GameCharacterController.CharacterActionType _actionType )
	{
		int animParamId = 0;
		int actionCount = m_SupportedAnimActions.Length;
		for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
		{
			CharacterAnimAction action = m_SupportedAnimActions[actionIndex];
			if (action.m_ActionType == _actionType)
			{
				animParamId = action.GetParamId();
				break;
			}
		}
		return animParamId;
	}

	void InitActions()
	{
		int actionCount = m_SupportedAnimActions.Length;
		for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
		{
			CharacterAnimAction action = m_SupportedAnimActions[actionIndex];
			action.InitParamId();
		}

		m_SpeedId = Animator.StringToHash("Speed");
		m_DirectionId = Animator.StringToHash("Direction");
	}

	void ResetActions()
	{
		int actionCount = m_SupportedAnimActions.Length;
		for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
		{
			CharacterAnimAction action = m_SupportedAnimActions[actionIndex];
			int animParamId = action.GetParamId();
			m_Animator.SetBool( animParamId, false );
		}
	}
	
	void InitCharacterCollisionUpdate()
	{
		CharacterController  charController = GetComponent<CharacterController>();
		m_BaseCapsuleHeight = charController.height;
		m_BaseCapsuleCenter = charController.center;
		
		m_JumpingCurveId = Animator.StringToHash("JumpingCurve");
	}

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

		InitActions();
	}

	void SetupGameCharacterController(GameCharacterController _GameCharacterController)
	{
		m_GameCharacterController = _GameCharacterController;
		
		InitCharacterCollisionUpdate();
	}

	void Update()
	{
		float deltaTime = Time.deltaTime;

		if (m_Animator && m_GameCharacterController)
		{
			if (m_GameCharacterController.CanUpdateCollision())
			{
				UpdateCharacterCollisionFromAnimCurves(m_Animator, m_GameCharacterController);
			}
			
			ResetActions();

			List<GameCharacterController.CharacterActionType> startedActions = m_GameCharacterController.GetActions();
			foreach (GameCharacterController.CharacterActionType action in startedActions)
			{
				int animParamId = FindAnimParamId(action);
				m_Animator.SetBool( animParamId, true );
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
		}
	}
	
	void UpdateCharacterCollisionFromAnimCurves(
		Animator _Animator,
		GameCharacterController _GameCharacterController)
	{
		Vector3 center = m_BaseCapsuleCenter + new Vector3(0.0f, m_BaseCapsuleCenter.y * m_JumpCapsuleOffsetY * _Animator.GetFloat(m_JumpingCurveId), 0.0f);
		float height = m_BaseCapsuleHeight * (1 + m_JumpCapsuleScaleFactor * _Animator.GetFloat(m_JumpingCurveId));
		
		_GameCharacterController.UpdateCollision(center, height);
	}

	public override bool IsJumping()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isJumping = stateInfo.IsName("Base Layer.Jump");

		AnimatorStateInfo nextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
		bool isGoingToJump = nextStateInfo.IsName("Base Layer.Jump");

		return (isJumping || isGoingToJump);
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
	
	public override bool CanOrientationBeModified()
	{
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool isRunning = stateInfo.IsName("Base Layer.Run");
		bool isIdle = stateInfo.IsName("Base Layer.Idle");

		AnimatorStateInfo nextStateInfo = m_Animator.GetNextAnimatorStateInfo(0);
		bool isGoingToJump = nextStateInfo.IsName("Base Layer.Jump");
		bool isGoingToDive = nextStateInfo.IsName("Base Layer.Dive");
		bool isGoingToVault = nextStateInfo.IsName("Base Layer.Vault");

		return (isRunning || isIdle) && !(isGoingToJump || isGoingToDive || isGoingToVault);
	}
}
