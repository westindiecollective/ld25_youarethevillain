using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkSyncPlayer : MonoBehaviour
{
	bool m_SyncReceived = false;
	
	NetworkManager m_NetworkManager = null;
	PlayerManager m_PlayerManager = null;
	
	GameCharacterController m_CharacterController = null;
	CharacterAnimController m_AnimController = null;
	float m_SyncInputSpeed = 0.0f;
	float m_SyncInputLeftRightDirection = 0.0f;
	
	Vector3 m_SyncPosition = Vector3.zero;
	Quaternion m_SyncRotation = Quaternion.identity;
	
	[System.Serializable]
	public class SyncCharacterAction
	{
		public CharacterActionType m_ActionType = CharacterActionType.E_ActionNone;
		public Vector3 m_StartPos = Vector3.zero;
		public Quaternion m_StartRot = Quaternion.identity;
		
		public SyncCharacterAction(CharacterActionType _ActionType, Vector3 _StartPos, Quaternion _StartRot)
		{
			m_ActionType = _ActionType;
			m_StartPos = _StartPos;
			m_StartRot = _StartRot;
		}
	}
	List<SyncCharacterAction> m_SyncActions = null;

	void Start()
	{
		m_NetworkManager = (NetworkManager)FindObjectOfType( typeof(NetworkManager) );
		m_PlayerManager = (PlayerManager)FindObjectOfType( typeof(PlayerManager) );
		m_CharacterController = GetComponent<GameCharacterController>();
		m_AnimController = GetComponent<CharacterAnimController>();
		
		System.Diagnostics.Debug.Assert(m_CharacterController != null);
		System.Diagnostics.Debug.Assert(m_AnimController != null);
		
		m_SyncActions = new List<SyncCharacterAction>();
	}
	
	void Update()
	{
		bool isSyncPlayerAuthority = networkView.isMine;
		if (isSyncPlayerAuthority)
		{
			List<CharacterActionType> actions = m_CharacterController.GetActions();	
			int actionCount = actions.Count;
			if (actionCount > 0)
			{
				bool isNetAuthority = m_NetworkManager.IsNetworkAuthorithy();
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				
				Vector3 pos = m_CharacterController.GetPosition();
				Quaternion rot = m_CharacterController.GetOrientation();
				
				foreach(CharacterActionType action in actions)
				{
					int actionType = (int)action;
					
					if (isNetAuthority)
					{
						SyncPlayerStartActionServer(localNetClient, actionType, pos, rot);
					}
					else
					{
						networkView.RPC("SyncPlayerStartActionServer", RPCMode.Server, localNetClient, actionType, pos, rot);
					}
				}
			}
		}
		else
		{
			bool startingAction = false;
			
			int syncActionCount = m_SyncActions.Count;
			if (syncActionCount > 0)
			{
				startingAction = true;
				
				SyncCharacterAction firstSyncAction = m_SyncActions[0];
				
				List<CharacterActionType> actions = new List<CharacterActionType>();
				foreach(SyncCharacterAction syncAction in m_SyncActions)
				{
					actions.Add(syncAction.m_ActionType);
				}
				
				//@FIXME in the future, this system might need to handle multiple actions
				//what about the start position/rotation then?
				m_CharacterController.OverrideActions(actions);
				m_CharacterController.SetPosition(firstSyncAction.m_StartPos);
				m_CharacterController.SetOrientation(firstSyncAction.m_StartRot);
				
				m_SyncActions.Clear();
			}
			else
			{
				m_CharacterController.OverrideActions(null);
			}
			
			if (m_SyncReceived)
			{
				m_CharacterController.OverrideInputSpeed(m_SyncInputSpeed);
				m_CharacterController.OverrideInputLeftRightDirection(m_SyncInputLeftRightDirection);
				
				bool animIsInAction = (m_AnimController != null) && (m_AnimController.IsInAction());
				if ( (startingAction == false) && (animIsInAction == false) )
				{
					m_CharacterController.SetPosition(m_SyncPosition);
					m_CharacterController.SetOrientation(m_SyncRotation);
				}
			}
		}
		
		m_SyncReceived = false;
	}
	
	[RPC]
	void SyncPlayerStartActionServer(NetworkPlayer _SyncPlayerAuthority, int _ActionType, Vector3 _StartPos, Quaternion _StartRot)
	{
		List<NetworkPlayer> netPlayingClients = new List<NetworkPlayer>();
		
		List<Player> players = m_PlayerManager.GetPlayers();
		foreach(Player player in players)
		{
			NetworkPlayer netClient = player.m_NetClient;
			bool isSyncPlayerAuthority = ( netClient == _SyncPlayerAuthority );
			bool isPlaying = ( player.m_PlayerInstance != null );
			bool isClientListed = netPlayingClients.Contains(netClient);
			
			if ( !isSyncPlayerAuthority && isPlaying && !isClientListed)
			{
				netPlayingClients.Add(netClient);
			}
		}
		
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		foreach(NetworkPlayer netClient in netPlayingClients)
		{
			bool isRemoteClient = (netClient != localNetClient);
			if (isRemoteClient)
			{
				networkView.RPC("SyncPlayerStartActionRemoteClient", netClient,  _ActionType, _StartPos, _StartRot);
			}
			else
			{
				SyncPlayerStartActionRemoteClient(_ActionType, _StartPos, _StartRot);
			}
		}
		
		netPlayingClients.Clear();
	}
	
	[RPC]
	void SyncPlayerStartActionRemoteClient(int _ActionType, Vector3 _StartPos, Quaternion _StartRot)
	{
		CharacterActionType actionType = (CharacterActionType)_ActionType;
		
		SyncCharacterAction syncAction = new SyncCharacterAction(actionType, _StartPos, _StartRot);
		m_SyncActions.Add(syncAction);
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (m_SyncActions != null)
		{
			if (stream.isWriting)
			{
				float inputSpeed = m_CharacterController.GetInputSpeed();
				float inputLeftRightDirection = m_CharacterController.GetInputLeftRightDirection();
				stream.Serialize(ref inputSpeed);
				stream.Serialize(ref inputLeftRightDirection);
				
				Vector3 pos = m_CharacterController.GetPosition();
				Quaternion rot = m_CharacterController.GetOrientation();
				stream.Serialize(ref pos);
				stream.Serialize(ref rot);
			}
			else
			{
				float inputSpeed = 0.0f;
				float inputLeftRightDirection = 0.0f;
				stream.Serialize(ref inputSpeed);
				stream.Serialize(ref inputLeftRightDirection);
				
				m_SyncInputSpeed = inputSpeed;
				m_SyncInputLeftRightDirection = inputLeftRightDirection;
				
				Vector3 pos = Vector3.zero;
				Quaternion rot = Quaternion.identity;
				stream.Serialize(ref pos);
				stream.Serialize(ref rot);
				
				m_SyncPosition = pos;
				m_SyncRotation = rot;
				
				m_SyncReceived = true;
			}
		}
	}
}
