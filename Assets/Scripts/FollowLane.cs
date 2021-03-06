#define ALWAYS_PROCESS_FOLLOW_LANE
//#define DEBUG_FOLLOW_LANE

using UnityEngine;
using System.Collections;

public class FollowLane : MonoBehaviour
{
	public int m_LaneCount = 1;
	public int m_TargetLaneIndex = 0;

	public float m_LanesWidth = 1.0f;
	public Vector3 m_LanesLeftOrigin = Vector3.zero;
	public Vector3 m_LanesForwardDir = Vector3.forward;
	public Vector3 m_LanesLeftToRightDir = Vector3.right;

	public float m_LaneChangeTimeFactor = 1.0f;

	public float m_DistanceToLaneThreshold = 0.01f;

	public float m_MaxAngularSpeed = Mathf.Infinity;

	public float m_ChangeLaneInputThreshold = 0.8f;
	public int m_ChangeLaneInputFrameCountThreshold = 1;

	GameCharacterController m_GameCharacterController = null;

#if !DEBUG_FOLLOW_LANE
	bool m_CanChangeLane = false;
	bool m_WantToChangeLaneOnLeft = false;
	bool m_WantToChangeLaneOnRight = false;
	int m_ChangeLaneInputFrameCount = 0;

	bool m_IsChangingLane = false;

	Vector3 m_CharacterAngularVelocity = Vector3.zero;
#else
	public bool m_CanChangeLane = false;
	public bool m_WantToChangeLaneOnLeft = false;
	public bool m_WantToChangeLaneOnRight = false;
	public int m_ChangeLaneInputFrameCount = 0;

	public bool m_IsChangingLane = false;

	public Vector3 m_CharacterAngularVelocity = Vector3.zero;

	public GameObject m_DebugPlayer = null;
	public GameObject m_DebugTarget = null;
	public float m_DebugLaneChangeStartTime = 0.0f;
	public int m_DebugLaneChangeStartFrame = 0;
#endif
	
	public void EnableFollowLaneUpdate()
	{
		enabled = true;	
	}
	
	public void DisableFollowLaneUpdate()
	{
		enabled = false;
	}
	
	void Start ()
	{
		m_GameCharacterController = GetComponent<GameCharacterController>();
		
		DisableFollowLaneUpdate();
	}

	void Update ()
	{
		float deltaTime = Time.deltaTime;
		
		UpdateFollowLane(deltaTime);
	}
	
	private void UpdateFollowLane(float _DeltaTime)
	{
		if (m_CanChangeLane && m_GameCharacterController)
		{
			GameObject character = m_GameCharacterController.gameObject;
			Vector3 characterPos = m_GameCharacterController.GetPosition();
			float laneWidth = m_LanesWidth / m_LaneCount;
			
			float h = m_GameCharacterController.GetInputLeftRightDirection();

			int currentLaneIndex = m_TargetLaneIndex;
			int firstLaneIndex = 0;
			int lastLaneIndex = m_LaneCount - 1;

			bool changeLeft = m_WantToChangeLaneOnLeft;
			bool changeRight = m_WantToChangeLaneOnRight;
			int frame_count = m_ChangeLaneInputFrameCount;
			int frame_count_min = m_ChangeLaneInputFrameCountThreshold;

			float h_min = m_ChangeLaneInputThreshold;
			if ( h < -h_min )
			{
				frame_count = (changeLeft)? frame_count+1 : 1;
				changeLeft = (currentLaneIndex > firstLaneIndex) && !WillCollideWhenChangingLane(-m_LanesLeftToRightDir, laneWidth, characterPos, character);
				changeRight = false;
			}
			else if ( h > h_min )
			{
				frame_count = (changeRight)? frame_count+1 : 1;
				changeLeft = false;
				changeRight = (currentLaneIndex < lastLaneIndex) && !WillCollideWhenChangingLane(m_LanesLeftToRightDir, laneWidth, characterPos, character);;
			}
			else
			{
				frame_count = 0;
				changeLeft = false;
				changeRight = false;
			}

			if ( (changeLeft || changeRight) && (frame_count >= frame_count_min) )
			{
				ChangeLane( changeLeft, h, _DeltaTime );
				m_CanChangeLane = false;
				
				//@NOTE: preventing players changing lane from performing an action is too restrictive
				//this would prevent players from jumping just because they're changing lane
				//m_GameCharacterController.DisableActions();
			}

			m_ChangeLaneInputFrameCount = frame_count;
			m_WantToChangeLaneOnLeft = changeLeft;
			m_WantToChangeLaneOnRight = changeRight;
		}

		//@HACK: since jump anims aren't perfectly aligned/straight,
		//correcting player orientation during a jump anim introduce visible anim jigger artifacts
		CharacterAnimController animComponent = GetComponent<CharacterAnimController>();
		bool canModifyOrientation = ((animComponent != null) && (animComponent.IsInAction() == false));
		bool updateFollowLane = (m_LaneCount > 0) && canModifyOrientation;

		if (updateFollowLane)
		{
			float laneWidth = m_LanesWidth / m_LaneCount;
			Vector3 laneCenterAtOrigin = ComputeLaneCenterAtOrigin(m_TargetLaneIndex, m_LanesLeftOrigin, m_LanesLeftToRightDir, laneWidth);

			Vector3 currentPos = m_GameCharacterController.GetPosition();
			Quaternion currentRot = m_GameCharacterController.GetOrientation();
			Vector3 laneCenter = ComputeLaneCenterForPos( currentPos, laneCenterAtOrigin, m_LanesLeftToRightDir);

#if DEBUG_FOLLOW_LANE
			//Unity follows left handed convention
			Vector3 laneUpDir = Vector3.Cross(m_LanesForwardDir, m_LanesLeftToRightDir);

			if (m_DebugPlayer)
			{
				currentPos = m_DebugPlayer.transform.position;
				currentRot = m_DebugPlayer.transform.rotation;
				Vector3 laneCenterDebug = ComputeLaneCenterForPos( currentPos, laneCenterAtOrigin, m_LanesLeftToRightDir);
				laneCenter = laneCenter + Vector3.Dot( laneCenterDebug - laneCenter, laneUpDir ) * laneUpDir;
			}
#endif

			float distanceToLane = ComputeDistanceToLane( currentPos, laneCenterAtOrigin, m_LanesLeftToRightDir);

#if !ALWAYS_PROCESS_FOLLOW_LANE
			if (distanceToLane > m_DistanceToLaneThreshold)
#endif
			{
				float characterVelocity = m_GameCharacterController.GetVelocity();
				
				//The idea is that when the character is blocked by an obstacle
				// it shouldn't target ahead but go straight towards the lane center
				// but it should always target forward when it's already close to the lane center (whether velocity is null or not)
				float targetForwardOffset = (distanceToLane > m_DistanceToLaneThreshold)? 0.0f : 1.0f;
				if ( characterVelocity * m_LaneChangeTimeFactor > laneWidth )
				{
					float laneChangeDistance = characterVelocity * m_LaneChangeTimeFactor; //approximation
					targetForwardOffset = laneChangeDistance * Mathf.Max (distanceToLane / laneWidth, 1.0f); //always have target ahead to help with rotation smoothing
				}

				Vector3 targetPos = laneCenter + targetForwardOffset * m_LanesForwardDir;
#if DEBUG_FOLLOW_LANE
				if (m_DebugTarget)
				{
					m_DebugTarget.transform.position = targetPos;
				}
#endif
				float smoothTime = (distanceToLane / laneWidth) * m_LaneChangeTimeFactor;

				Vector3 targetDir = Vector3.Normalize(targetPos - currentPos);
				Quaternion targetRot = Quaternion.LookRotation(targetDir);

				//Quaternion newRot = Quaternion.Slerp(currentRot, targetRot, deltaTime * m_LaneChangeTimeFactor);

				float angularVelocityY = m_CharacterAngularVelocity.y;
				float curRotY = currentRot.eulerAngles.y;
				float targetRotY = targetRot.eulerAngles.y;
				float newRot2Y = Mathf.SmoothDampAngle(curRotY, targetRotY, ref angularVelocityY, smoothTime, m_MaxAngularSpeed, _DeltaTime);
#if DEBUG_FOLLOW_LANE
				//float targetAngleRotY = curRotY + Mathf.DeltaAngle(curRotY, targetRotY);
				//Debug.Log(string.Format("Rot Y - cur:{0}, target:{1}, targetangle:{2}, new:{3}", curRotY, targetRotY, targetAngleRotY, newRot2Y));
#endif
				Quaternion newRot2 = Quaternion.Euler(0.0f, newRot2Y, 0.0f);

#if DEBUG_FOLLOW_LANE
				if (m_DebugPlayer)
				{
					Vector3 newPos2 = currentPos + (characterVelocity * deltaTime) * (newRot2 * Vector3.forward);

					m_DebugPlayer.transform.rotation = newRot2;
					m_DebugPlayer.transform.position = newPos2;
				}
				else
#endif
				{
					m_GameCharacterController.SetOrientation(newRot2);
				}

				m_CharacterAngularVelocity.x = 0.0f;//angularVelocityX;
				m_CharacterAngularVelocity.y = angularVelocityY;
				m_CharacterAngularVelocity.z = 0.0f;//angularVelocityZ;
			}
#if ALWAYS_PROCESS_FOLLOW_LANE
			if (distanceToLane > m_DistanceToLaneThreshold)
			{

			}
#endif
			else
			{
#if !ALWAYS_PROCESS_FOLLOW_LANE
	#if DEBUG_FOLLOW_LANE
				if (m_DebugPlayer)
				{
					m_DebugPlayer.transform.position = laneCenter;
					m_DebugPlayer.transform.rotation = Quaternion.LookRotation( m_LanesForwardDir );
				}
				else
	#endif
				{
					m_GameCharacterController.SetOrientation( Quaternion.LookRotation(m_LanesForwardDir) );
				}
				m_CharacterAngularVelocity = Vector3.zero;
#endif	//	!ALWAYS_PROCESS_FOLLOW_LANE

				m_CanChangeLane = true;

				if (IsChangingLane())
				{
					ChangeLaneDone();

					m_GameCharacterController.EnableActions();
				}
			}
		}
		else
		{
			//free running mode
		}
	}
	
	bool WillCollideWhenChangingLane(Vector3 _ChangeLaneDir, float _LaneWidth, Vector3 _Position, GameObject _CharacterChangingLane)
	{
		bool willCollide = false;
		
		RaycastHit[] hits;
        hits = Physics.RaycastAll(_Position, _ChangeLaneDir, _LaneWidth);
		
        foreach (RaycastHit hit in hits)
		{
			bool isTrigger = hit.collider.isTrigger;
			GameObject blockingObject = hit.collider.gameObject;
			
			if (!isTrigger && blockingObject != _CharacterChangingLane)
			{
				//Debug.Log(string.Format("Character {0} can't change lane because of object {1}.", _CharacterChangingLane, blockingObject));
				willCollide = true;
				break;
			}
        }
		
		return willCollide;
    }

	bool IsChangingLane()
	{
		return m_IsChangingLane;
	}

	void ChangeLane(bool _ChangeLaneOnLeft, float _InputValue, float _DeltaTime)
	{	
		if (_ChangeLaneOnLeft)
		{
			--m_TargetLaneIndex;
			System.Diagnostics.Debug.Assert( m_TargetLaneIndex >= 0);
		}
		else
		{
			++m_TargetLaneIndex;
			System.Diagnostics.Debug.Assert( m_TargetLaneIndex < m_LaneCount);
		}

#if DEBUG_FOLLOW_LANE
		m_DebugLaneChangeStartTime = Time.time;
		m_DebugLaneChangeStartFrame = Time.frameCount;
#endif

		m_IsChangingLane = true;
	}

	void ChangeLaneDone()
	{
#if DEBUG_FOLLOW_LANE
		float debugLaneChangeEndTime = Time.time;
		int debugLaneChangeEndFrame = Time.frameCount;
		float debugLaneChangeTime = debugLaneChangeEndTime - m_DebugLaneChangeStartTime;
		int debugLaneChangeFrameCount = debugLaneChangeEndFrame - m_DebugLaneChangeStartFrame;
		//Debug.Log(string.Format("Lane change - time: {0}, frame count: {1}", debugLaneChangeTime, debugLaneChangeFrameCount));
#endif

		m_IsChangingLane = false;
	}

	Vector3 ComputeLaneCenterAtOrigin(int _LaneIndex, Vector3 _LanesLeftOrigin, Vector3 _LanesLeftToRightDir, float _LaneWidth)
	{
		float laneCenterOffsetRight = (_LaneIndex + 0.5f) * _LaneWidth;
		Vector3 laneCenterAtOrigin = _LanesLeftOrigin + laneCenterOffsetRight * _LanesLeftToRightDir;

		return laneCenterAtOrigin;
	}

	Vector3 ComputeLaneCenterForPos(Vector3 _CurrentPos, Vector3 _LaneCenterAtOrigin, Vector3 _LanesLeftToRightDir)
	{
		Vector3 laneCenterForPos = _CurrentPos + Vector3.Dot(_LaneCenterAtOrigin - _CurrentPos, _LanesLeftToRightDir) * _LanesLeftToRightDir;

		return laneCenterForPos;
	}

	float ComputeDistanceToLane(Vector3 _CurrentPos, Vector3 _LaneCenterAtOrigin, Vector3 _LanesLeftToRightDir)
	{
		float distanceToLane = Mathf.Abs( Vector3.Dot(_LaneCenterAtOrigin - _CurrentPos, _LanesLeftToRightDir) );
		return distanceToLane;
	}
}
