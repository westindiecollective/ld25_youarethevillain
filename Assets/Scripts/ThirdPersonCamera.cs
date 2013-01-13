using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public float m_DistanceAway;    // distance from the back of the craft
	public float m_DistanceUp;      // distance above the craft
	public float m_SmoothFactor;    // how smooth the camera movement is

	public bool m_UseLookDir = false;
	public bool m_UseLookDirAsFollowPositionMask = false;
	public Vector3 m_LookDir = Vector3.forward;

	private Vector3 m_TargetPosition;   // the position the camera is trying to be in

	Transform m_FollowTransform;

	void Start()
	{
		m_FollowTransform = GameObject.FindWithTag ("Player").transform;
	}

	void LateUpdate()
	{
		float deltaTime = Time.deltaTime;

		// make sure the camera is looking the right way!
		if (m_UseLookDir)
		{
			transform.rotation = Quaternion.LookRotation(m_LookDir);
		}
		else
		{
			transform.LookAt(m_FollowTransform);
		}

		Vector3 lookDir = (m_UseLookDir)? m_LookDir : m_FollowTransform.forward;
		Vector3 followPos = (m_UseLookDirAsFollowPositionMask)? Vector3.Dot(m_FollowTransform.position, m_LookDir) * m_LookDir: m_FollowTransform.position;

		// setting the target position to be the correct offset from the hovercraft
		m_TargetPosition = followPos + m_DistanceUp * Vector3.up  - m_DistanceAway * lookDir;

		// making a smooth transition between it's current position and the position it wants to be in
		transform.position = Vector3.Lerp(transform.position, m_TargetPosition, deltaTime * m_SmoothFactor);
	}
}
