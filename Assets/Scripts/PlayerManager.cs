using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerInputDevice { E_PlayerInputInvalid, E_PlayerMouseKeyboad, E_PlayerGamepad_1, E_PlayerGamepad_2, E_PlayerTouch };

public class Player
{
	public NetworkPlayer m_NetClient;
	public NetworkViewID m_NetViewID;
	public PlayerInputDevice m_LocalPlayerInput;
	public GameObject m_PlayerInstance;
	
	public Player(PlayerInputDevice _LocalPlayerInput)
	{
		m_LocalPlayerInput = _LocalPlayerInput;
		m_PlayerInstance = null;
	}
	
	public Player(NetworkPlayer _NetClient, NetworkViewID _NetViewID, PlayerInputDevice _LocalPlayerInput)
	{
		m_NetClient = _NetClient;
		m_NetViewID = _NetViewID;
		m_LocalPlayerInput = _LocalPlayerInput;
		m_PlayerInstance = null;
	}
}

public class PlayerManager : MonoBehaviour
{
	private List<Player> m_Players = null;
	private List<Player> m_PendingLocalPlayers = null;
	
	public List<Player> GetPlayers() { return m_Players; }
	
	public List<Player> FindPlayersFromClient(NetworkPlayer _NetClient)
	{
		List<Player> clientPlayers = new List<Player>();
		foreach (Player joinedPlayer in m_Players)
		{
			if (joinedPlayer.m_NetClient == _NetClient)
			{
				clientPlayers.Add(joinedPlayer);
			}
		}
		
		return clientPlayers;
	}
	
	public Player FindPlayer(NetworkViewID _NetViewID)
	{
		Player playerFound = null;
		foreach (Player player in m_Players)
		{
			if (player.m_NetViewID == _NetViewID)
			{
				playerFound = player;
				break;
			}
		}
		
		return playerFound;
	}
	
	public bool HasLocalPlayerJoined(NetworkPlayer _LocalNetClient, PlayerInputDevice _PlayerInput)
	{
		bool playerJoined = false;
		foreach (Player player in m_Players)
		{
			bool isLocalPlayer = (player.m_NetClient == _LocalNetClient);
			bool matchingInput = (player.m_LocalPlayerInput == _PlayerInput);
			
			if (isLocalPlayer && matchingInput)
			{
				playerJoined = true;
				break;
			}
		}
		
		return playerJoined;
	}
	
	public bool IsLocalPlayerJoining(NetworkPlayer _LocalNetClient, PlayerInputDevice _PlayerInput)
	{
		bool playerJoining = false;
		foreach ( Player joiningPlayer in m_PendingLocalPlayers )
		{
			bool isLocalPlayer = (joiningPlayer.m_NetClient == _LocalNetClient);
			bool matchingInput = (joiningPlayer.m_LocalPlayerInput == _PlayerInput);
			if (isLocalPlayer && matchingInput)
			{
				playerJoining = true;
				break;
			}
		}
		
		return playerJoining;
	}
	
	void Awake()
	{
		m_Players = new List<Player>();
		m_PendingLocalPlayers = new List<Player>();
	}
	
	void Start()
	{
	
	}
	
	void Update()
	{
	
	}
	
	private Player AddLocalPlayerPendingJoin(NetworkPlayer _LocalNetClient, PlayerInputDevice _PlayerInput)
	{
		NetworkViewID newViewID = Network.AllocateViewID();
		Player newLocalPlayer = new Player(_LocalNetClient, newViewID, _PlayerInput);
		m_PendingLocalPlayers.Add(newLocalPlayer);
		
		return newLocalPlayer;
	}
	
	public void AddLocalPlayerToJoin(NetworkPlayer _LocalNetClient, PlayerInputDevice _PlayerInput, bool _IsAuthority)
	{		
		Player pendingPlayer = AddLocalPlayerPendingJoin(_LocalNetClient, _PlayerInput);
		System.Diagnostics.Debug.Assert(pendingPlayer.m_NetClient == _LocalNetClient);
		
		Debug.Log("Adding local player to join (input: " + _PlayerInput.ToString() + ")");
		Debug.Log("IP: "+ _LocalNetClient.ipAddress + ", port:" + _LocalNetClient.port + ", NetViewID: " + pendingPlayer.m_NetViewID.ToString());
		
		if (_IsAuthority)
		{
			//@NOTE: Unity3d network system doesn't support sending RPCs to server from server when using RPCMode.Server (RPCMode.All is fine)
			PlayerJoinAuthority(pendingPlayer.m_NetClient, pendingPlayer.m_NetViewID);
		}
		else
		{
			if (m_Players.Count == 0)
			{
				networkView.RPC("ClientRequestPlayersJoinAuthority", RPCMode.Server, pendingPlayer.m_NetClient);
			}
			networkView.RPC("PlayerJoinAuthority", RPCMode.Server, pendingPlayer.m_NetClient, pendingPlayer.m_NetViewID);
		}
	}
	
	[RPC]
	public void ClientRequestPlayersJoinAuthority(NetworkPlayer _NetClient)
	{
		Debug.Log("Client requested players join on Authority");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port);
		
		List<Player> players = m_Players;
		foreach (Player player in players)
		{
			networkView.RPC("PlayerJoin", _NetClient, player.m_NetClient, player.m_NetViewID);
		}
	}
	
	[RPC]
	public void PlayerJoinAuthority(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Debug.Log("Player join on Authority");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
		
		//@TODO: any team balancing or sthg to do here?
		//Remove NPC AIs if needed?
		
		//@FIXME: handle case where server is full?

		//player join on server
		PlayerJoin(_NetClient, _NetViewID);
		
		//player join on clients
		networkView.RPC("PlayerJoin", RPCMode.Others, _NetClient, _NetViewID);
	}
	
	[RPC]
	public void PlayerJoin(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		bool localPlayerJoining = (Network.player == _NetClient);
		if (localPlayerJoining)
		{
			LocalPlayerJoin(_NetClient, _NetViewID);
		}
		else
		{
			RemotePlayerJoin(_NetClient, _NetViewID);
		}
	}
	
	private void LocalPlayerJoin(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Player joiningPlayer = null;
		foreach(Player pendingPlayer in m_PendingLocalPlayers)
		{
			if (pendingPlayer.m_NetViewID == _NetViewID)
			{
				System.Diagnostics.Debug.Assert(joiningPlayer.m_NetClient == _NetClient);
				
				joiningPlayer = pendingPlayer;
				break;
			}
		}
		System.Diagnostics.Debug.Assert(joiningPlayer != null);
		
		if (joiningPlayer != null)
		{
			m_Players.Add(joiningPlayer);
			m_PendingLocalPlayers.Remove(joiningPlayer);
			
			Debug.Log("Local player joined (input: " + joiningPlayer.m_LocalPlayerInput.ToString() + ")");
			Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
		}
		else
		{
			Debug.Log("Local player couldn't join: missing from the local players pending list!");
			Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
		}
	}
	
	private void RemotePlayerJoin(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Player joiningPlayer = new Player(_NetClient, _NetViewID, PlayerInputDevice.E_PlayerInputInvalid);
		m_Players.Add(joiningPlayer);
		
		Debug.Log("Remote player joined");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
	}
}
