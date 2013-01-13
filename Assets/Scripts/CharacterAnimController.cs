using UnityEngine;
using System.Collections;

public abstract class CharacterAnimController : AnimationController
{
	public abstract bool IsJumping();

	public abstract bool IsRunning();

	public abstract bool IsThrowing();
}
