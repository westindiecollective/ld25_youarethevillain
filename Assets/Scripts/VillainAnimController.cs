using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VillainAnimController : CharacterAnimController
{
	public CharacterAnimAction[] m_SupportedAnimActions;
	public string[] m_baseAnimatorStates;
	
	int[] m_baseAnimatorStateIds = null;

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
	int m_CollisionHeightScaleCurveId = 0;

	int FindAnimParamId( CharacterActionType _actionType )
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
		
		int animStateCount = m_baseAnimatorStates.Length;
		
		m_baseAnimatorStateIds = new int[animStateCount];
		for (int animStateIndex = 0; animStateIndex < animStateCount; ++animStateIndex)
		{
			int animStateId = Animator.StringToHash(m_baseAnimatorStates[animStateIndex]);
			m_baseAnimatorStateIds[animStateIndex] = animStateId;
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
		
		m_CollisionHeightScaleCurveId = Animator.StringToHash("CollisionHeightScaleCurve");
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
			UpdateCharacterCollisionFromAnimCurves(m_Animator, m_GameCharacterController, m_BaseCapsuleCenter, m_BaseCapsuleHeight);
			
			ResetActions();

			List<CharacterActionType> startedActions = m_GameCharacterController.GetActions();
			foreach (CharacterActionType action in startedActions)
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
		GameCharacterController _GameCharacterController,
		Vector3 _BaseCollisionCenter,
		float _BaseCollisionHeight)
	{
		if (_GameCharacterController.CanUpdateCollisionCenter())
		{
			Vector3 center = _BaseCollisionCenter;
			_GameCharacterController.UpdateCollisionCenter(center);
		}
		
		//float height = _BaseCollisionHeight * _Animator.GetFloat(m_CollisionHeightScaleCurveId);
		//_GameCharacterController.UpdateCollisionHeight(height);
	}
	
	public override bool IsInAction()
	{
		int actionAnimLayerIndex = 0;
		
		AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(actionAnimLayerIndex);
		int currentAnimStateId = stateInfo.nameHash;
		
		bool isInBaseAnimState = false;
		foreach(int baseAnimStateId in m_baseAnimatorStateIds)
		{
			if (currentAnimStateId == baseAnimStateId)
			{
				isInBaseAnimState = true;
				break;
			}
		}
		
		//bool isRunning = stateInfo.IsName("Base Layer.Run");
		//bool isIdle = stateInfo.IsName("Base Layer.Idle");

		AnimatorStateInfo nextStateInfo = m_Animator.GetNextAnimatorStateInfo(actionAnimLayerIndex);
		int nextAnimStateId = nextStateInfo.nameHash;
		
		bool willBeInBaseAnimState = isInBaseAnimState;
		
		bool hasNextAnimState = (nextAnimStateId != 0);
		if (hasNextAnimState)
		{
			willBeInBaseAnimState = false;
			foreach(int baseAnimStateId in m_baseAnimatorStateIds)
			{
				if (nextAnimStateId == baseAnimStateId)
				{
					willBeInBaseAnimState = true;
					break;
				}
			}
		}

		return (isInBaseAnimState == false) || (willBeInBaseAnimState == false);
	}
}
