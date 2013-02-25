using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GameCharacterController : MonoBehaviour
{
	public enum CharacterActionType {
		E_ActionNone,
		E_ActionDive,
		E_ActionJump,
		E_ActionTakeHit,
		E_ActionUse,
		E_ActionVault,
	};

	[System.Serializable]
	public class CharacterAction
	{
		public CharacterActionType m_ActionType = GameCharacterController.CharacterActionType.E_ActionNone;
		public string m_ActionButton;
		public bool m_ActionIsImmediate;
		public Texture2D m_ActionIcon;
	}

	public abstract float GetInputSpeed();
	public abstract float GetInputLeftRightDirection();

	public abstract Vector3 GetPosition();
	public abstract void SetPosition(Vector3 _Position);
	public abstract Quaternion GetOrientation();
	public abstract void SetOrientation(Quaternion _Orientation);
	public abstract float GetVelocity();
	
	public abstract void UpdateCollision(Vector3 _Center, float _Height);
	public abstract bool CanUpdateCollision();
	public abstract void AuthorizeUpdatingCollision();
	public abstract void UnauthorizeUpdatingCollision();

	public abstract void EnableActions();
	public abstract void DisableActions();
	public abstract void EnableAction(CharacterActionType _Action);
	public abstract void DisableAction(CharacterActionType _Action);
	public abstract List<CharacterActionType> GetActions();
	public abstract void TriggerPendingAction(CharacterActionType _Action);

	public abstract void HandleHit();

}
