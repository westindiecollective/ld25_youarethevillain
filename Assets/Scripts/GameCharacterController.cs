using UnityEngine;
using System.Collections;

public abstract class GameCharacterController : MonoBehaviour
{
	public abstract float GetInputSpeed();
	public abstract float GetInputLeftRightDirection();

	public abstract Vector3 GetPosition();
	public abstract void SetPosition(Vector3 _Position);
	public abstract Quaternion GetOrientation();
	public abstract void SetOrientation(Quaternion _Orientation);
	public abstract float GetVelocity();

	public abstract void EnableActions();
	public abstract void DisableActions();
	public abstract bool IsStartingAction(int _ActionIndex);

	public abstract void HandleHit();

}
