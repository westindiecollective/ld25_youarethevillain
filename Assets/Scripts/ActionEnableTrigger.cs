using UnityEngine;
using System.Collections;

public class ActionEnableTrigger : MonoBehaviour
{
	public CharacterActionType m_ActionToActivate = CharacterActionType.E_ActionNone;

	void Start()
	{
	}

	void Update()
	{
	}

	void OnTriggerEnter(Collider other)
	{
		if (m_ActionToActivate != CharacterActionType.E_ActionNone)
		{
			//Debug.Log(string.Format("Game object {0} can now start action {1}.", other.gameObject.name, m_ActionToActivate.ToString()));
	
			GameCharacterController gameController = other.gameObject.GetComponent<GameCharacterController>();
			if (gameController)
			{
				gameController.EnableAction(m_ActionToActivate);
				gameController.UnauthorizeUpdatingCollisionCenter();
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (m_ActionToActivate != CharacterActionType.E_ActionNone)
		{
			//Debug.Log(string.Format("Game object {0} won't be able to start action {1}.", other.gameObject.name, m_ActionToActivate.ToString()));
	
			GameCharacterController gameController = other.gameObject.GetComponent<GameCharacterController>();
			if (gameController)
			{
				gameController.TriggerPendingAction(m_ActionToActivate);
				gameController.DisableAction(m_ActionToActivate);
				gameController.AuthorizeUpdatingCollisionCenter();
			}
		}
	}
}
