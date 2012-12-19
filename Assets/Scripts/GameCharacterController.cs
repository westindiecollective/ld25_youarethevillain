using UnityEngine;
using System.Collections;

public abstract class GameCharacterController : MonoBehaviour
{
	public abstract float GetSpeed();
	
	public abstract float GetLeftRightDirection();
	
	public abstract bool IsStartingAction(int _ActionIndex);
}
