using MLAPI;
using MLAPI.Messaging;
using MLAPI.Prototyping;
using MLAPI.Spawning;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public enum MatchSlot
{
    PG,
    SG,
    SF,
    PF,
    C
}

/// <summary>
/// GameSetup handles getting the game ready for play. Making sure players are connected. MatchGlobals are set.
/// </summary>
public class GameSetup : NetworkBehaviour
{
    private const string DEFAULT_LOADING_MSG = "Loading...";
    private const string NETWORK_LOADING_MSG = "Logging you in...";

    public bool isReady = false;

    private bool m_hasClientLoaded = false;
    private bool m_hasClientConnected = false;

    private void Start()
    {
    }

    public override void NetworkStart()
    {
        print("Starting GameSetup : " + gameObject.name);
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Initialize ai
            for (int tid = 0; tid < 2; tid++)
            {
                MatchTeam team = Match.matchTeams[tid];

                int aiToCreate = Match.MatchSettings.TeamSize - team.teamSize;

                for (int i = 0; i < aiToCreate; i++)
                {
                    GameObject go = Instantiate(ServerManager.Singleton.aiPrefab, Vector3.zero, Quaternion.identity);
                    GameObject modelObj = Instantiate(ServerManager.PrefabFromTeamID(tid), go.transform);

                    Player p = go.GetComponent<Player>();
                    Assert.IsNotNull(p, "aiLogic's Player component is null.");

                    p.InitilizeModel();

                    AIPlayer aiLogic = go.GetComponent<AIPlayer>();
                    Assert.IsNotNull(aiLogic, "aiPrefab in GameSetup does not have AIPlayer component");
                    
                    aiLogic.InitPlayer(p, tid);

                    go.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        m_hasClientLoaded = true;

        PlayerLoadedServerRpc(NetworkManager.Singleton.LocalClientId, ClientPlayer.Singleton.SteamID);
    }

    void Update()
    {
        isReady = (m_hasClientLoaded && m_hasClientConnected);
    }

    private void OnClientConnected(ulong steamId)
    {
        bool hasConnected = true;

        //ServerState.HandlePlayerConnection(steamId);

        ConnectedStatusClientRpc(hasConnected, RPCParams.ClientParamsOnlyClient(steamId));
    }

    [ClientRpc]
    private void ConnectedStatusClientRpc(bool hasConnected, ClientRpcParams cParams = default)
    {
        m_hasClientConnected = hasConnected;
    }

    [ServerRpc]
    public void PlayerLoadedServerRpc(ulong clientId, ulong steamId)
    {
        NetworkObject netObj = NetworkManager.ConnectedClients[clientId].PlayerObject;
        GameManager.Singleton.RegisterPlayer(netObj);
        PlayerLoadedClientRpc(RPCParams.ClientParamsOnlyClient(clientId));
    }

    [ClientRpc]
    public void PlayerLoadedClientRpc(ClientRpcParams cParams = default)
    {
        ClientPlayer.Singleton.State = ServerPlayerState.READY;
        GameManager.Singleton.LocalPlayerInitilized();
    }
}
