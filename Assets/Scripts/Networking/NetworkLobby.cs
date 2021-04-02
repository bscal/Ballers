using MLAPI;
using MLAPI.Messaging;
using System;
using UnityEngine;

public class NetworkLobby : MonoBehaviour
{

    public bool isDedicated;
    public bool usingDedicated;
    public string host = "";
    public int port = 7777;

    private void Awake()
    {
        Match.NetworkLobby = this;
    }

    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerReady;

        if (isDedicated)
        {
            NetworkManager.Singleton.StartServer();
        }
    }

    // Public Functions

    public void Connect()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Starting in client mode.");
    }

    public void HostServer()
    {
        Debug.Log("Starting in server mode.");
    }

    // Used for SteamP2P which was remvoed. Kept incase want to readd.
    public void SetSteamIDToConnect(ulong steamID)
    {
        //m_p2PTransport.ConnectToSteamID = steamID;
    }

    // Private Functions

    void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Approving...");
        //Your logic here
        bool approve = true;
        bool createPlayerObject = false;

        //ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator(""); // The prefab hash. Use null to use the default player prefab

        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, null, approve, Vector3.zero, Quaternion.identity);
    }

    void OnConnected(ulong client)
    {
        Debug.Log("Client Connected: " + client);
    }

    void OnDisconnected(ulong client)
    {
        Debug.Log("Client Disconnected: " + client);
    }

    void OnServerReady()
    {
        Debug.Log("Server Started");
    }

}