using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using MLAPI.Transports.SteamP2P;
using MLAPI.Messaging;
using System;
using MLAPI.Logging;

public class NetworkLobby : MonoBehaviour
{
    public bool hostServer = true;
    public string host = "";
    public int port = 7777;

    private SteamP2PTransport m_p2PTransport;

    private void Awake()
    {
        Match.NetworkLobby = this;
    }

    void Start()
    {
        m_p2PTransport = GetComponent<SteamP2PTransport>();
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerReady;
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

    public void SetSteamIDToConnect(ulong steamID)
    {
        m_p2PTransport.ConnectToSteamID = steamID;
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

    [ServerRpc]
    public void ClientLoadedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ServerPlayer sp = ServerManager.Singleton.players[serverRpcParams.Receive.SenderClientId];
        if (sp != null)
        {
            sp.state = ServerPlayerState.READY;
        }
    }

    [ServerRpc]
    public void SendIdsServerRpc(ulong steamId, int cid, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"Ids got : {clientId} | {steamId} | {cid}");
        if (ServerManager.Singleton.players.TryGetValue(clientId, out ServerPlayer sp))
        {
            sp.steamId = steamId;
            sp.cid = cid;

            Match.SetupPlayer(clientId, steamId, cid);

            sp.state = ServerPlayerState.IDLE;
            sp.hasBeenSetup = true;
        }
    }

    [ClientRpc]
    public void RequestIdsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("RequestIds got");
        ulong steam = ClientPlayer.Singleton.SteamID;
        int cid = ClientPlayer.Singleton.CharData.cid;
        SendIdsServerRpc(steam, cid);
    }
}