using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using MLAPI.Transports.SteamP2P;

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
        //NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = host;
        //NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = port;
        NetworkManager.Singleton.StartClient();
        Debug.Log("Starting in client mode.");
    }

    public void HostServer()
    {
        //NetworkManager.Singleton.StartHost();
        //NetworkManager.Singleton.StartClient();
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
        Debug.Log("Connected to " + NetworkManager.Singleton.ConnectedHostname);
    }

    void OnDisconnected(ulong client)
    {
        Debug.Log("Client Disconnected: " + client);
    }

    void OnServerReady()
    {
        Debug.Log("Server Started");
    }

    bool OnEvent()
    {
        return false;
    }
}