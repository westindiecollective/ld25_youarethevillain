using UnityEngine;
using System.Collections;

public class HitReceiverCharacter : HitReceiver
{
	void Start()
	{
	
	}

	void Update()
	{
	
	}

	public override void HandleHit()
	{
		//TODO: specify params to compute the impact
		
		GameCharacterController character = GetComponent<GameCharacterController>();
		character.HandleHit();
	}
}
