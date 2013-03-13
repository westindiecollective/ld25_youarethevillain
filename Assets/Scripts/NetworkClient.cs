using UnityEngine;
using System.Collections;

public class NetworkClient : MonoBehaviour
{
	public enum ClientState { E_ClientNone, E_ClientPendingConnect, E_ClientConnected, E_ClientPendingDisconnect, E_ClientDisconnected };
	private ClientState m_ClientState = ClientState.E_ClientNone;
	
	public delegate void ClientEventDelegate();
	private ClientEventDelegate m_ClientConnectedDelegate = null;
	private ClientEventDelegate m_ClientDisconnectedDelegate = null;
	
	private void ChangeClientState(ClientState _NewClientState) { m_ClientState = _NewClientState; }
	private bool CanConnectToServer() { return m_ClientState == ClientState.E_ClientNone; }

	void Start()
	{
	
	}
	
	void Update()
	{
	
	}
	
	public void ConnectToServer(string _ServerIpAdress, int _ConnectPort, ClientEventDelegate _ClientConnectedDelegate, ClientEventDelegate _ClientDisconnectedDelegate)
	{
		Debug.Log("Connecting to server " + _ServerIpAdress + ":" + _ConnectPort);
		
		System.Diagnostics.Debug.Assert(Network.peerType == NetworkPeerType.Disconnected);
		System.Diagnostics.Debug.Assert(CanConnectToServer());
		
		m_ClientConnectedDelegate = _ClientConnectedDelegate;
		m_ClientDisconnectedDelegate = _ClientDisconnectedDelegate;
		
		ChangeClientState(ClientState.E_ClientPendingConnect);
		
    	NetworkConnectionError connectionResult = Network.Connect(_ServerIpAdress, _ConnectPort);
		if (connectionResult != NetworkConnectionError.NoError)
		{
			Debug.Log("Connection failed:" + connectionResult);
		}
	}
	
	void OnConnectedToServer()
	{
		Debug.Log("Connected to server");
		
		ChangeClientState(ClientState.E_ClientConnected);
		
		if (m_ClientConnectedDelegate != null)
		{
			m_ClientConnectedDelegate();
			m_ClientConnectedDelegate = null;
		}
	}
	
	void OnFailedToConnect(NetworkConnectionError error) 
	{
        Debug.Log("Failed to connect to server: " + error);
		
		ChangeClientState(ClientState.E_ClientDisconnected);
		
		//@FIXME: what to do exactly about connected and disconnected delegates?
		m_ClientConnectedDelegate = null;
		if (m_ClientDisconnectedDelegate != null)
		{
			m_ClientDisconnectedDelegate();
			m_ClientDisconnectedDelegate = null;
		}
    }
	
	public void DisconnectFromServer(int _DisconnectTimeOutInMilliseconds)
	{
		Debug.Log("Disconnecting from server");
		
		ChangeClientState(ClientState.E_ClientPendingDisconnect);
		
		Network.Disconnect(_DisconnectTimeOutInMilliseconds);
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		Debug.Log("Server connection disconnected: " + info.ToString());
		Debug.Log("Current network peer type is " + Network.peerType.ToString());
		
		ChangeClientState(ClientState.E_ClientDisconnected);
		
		if (m_ClientDisconnectedDelegate != null)
		{
			m_ClientDisconnectedDelegate();
			m_ClientDisconnectedDelegate = null;
		}
    }
}
