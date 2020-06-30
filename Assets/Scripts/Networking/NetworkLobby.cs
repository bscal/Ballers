using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;

public class NetworkLobby : MonoBehaviour
{

    private static NetworkEvents m_networkEvents;

    public bool hostServer = true;
    public string host = "";
    public int port = 7777;

    private SteamP2PTransport.SteamP2PTransport m_p2PTransport;

    private void Awake()
    {
        m_networkEvents = GameObject.Find("NetworkManager").GetComponent<NetworkEvents>();
        Match.NetworkLobby = this;
    }

    void Start()
    {
        m_p2PTransport = GetComponent<SteamP2PTransport.SteamP2PTransport>();
        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        NetworkingManager.Singleton.OnServerStarted += OnServerReady;
    }

    // Public Functions

    public void Connect()
    {
        NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectAddress = host;
        NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectPort = port;
        NetworkingManager.Singleton.StartClient();
        Debug.Log("Starting in client mode.");
    }

    public void HostServer()
    {
        NetworkingManager.Singleton.StartHost();
        Debug.Log("Starting in server mode.");
    }

    public void SetSteamIDToConnect(ulong steamID)
    {
        m_p2PTransport.ConnectToSteamID = steamID;
    }

    // Private Functions

    void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Approving...");
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;

        ulong? prefabHash = SpawnManager.GetPrefabHashFromIndex(0); // The prefab hash. Use null to use the default player prefab

        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, prefabHash, approve, Vector3.zero, Quaternion.identity);
    }

    void OnConnected(ulong client)
    {
        Debug.Log("Client Connected: " + client);
        Debug.Log("Connected to " + NetworkingManager.Singleton.ConnectedHostname);
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

    public static NetworkEvents GetNetworkEvents()
    {
        return m_networkEvents;
    }
}
