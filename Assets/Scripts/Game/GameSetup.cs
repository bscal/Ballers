using MLAPI;
using MLAPI.Messaging;
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
public class GameSetup : NetworkedBehaviour
{
    private const string DEFAULT_LOADING_MSG = "Loading...";
    private const string NETWORK_LOADING_MSG = "Logging you in...";

    public bool isReady = false;

    public GameObject playerPrefab;
    public GameObject aiPrefab;

    public GameObject redPrefab;
    public GameObject bluePrefab;

    private bool m_hasClientLoaded = false;
    private bool m_hasClientConnected = false;

    private void Start()
    {
        if (Match.HostServer)
            Match.NetworkLobby.HostServer();
        else
            Match.NetworkLobby.Connect();
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Initialize ai
            for (int tid = 0; tid < 2; tid++)
            {
                MatchTeam team = Match.matchTeams[tid];

                int aiToCreate = Match.MatchSettings.TeamSize - team.teamSize;

                for (int i = 0; i < aiToCreate; i++)
                {
                    GameObject go = Instantiate(aiPrefab, Vector3.zero, Quaternion.identity);
                    GameObject modelObj = Instantiate(PrefabFromTeamID(tid), go.transform);

                    Player p = go.GetComponent<Player>();
                    Assert.IsNotNull(p, "aiLogic's Player component is null.");

                    p.InitilizeModel();

                    AIPlayer aiLogic = go.GetComponent<AIPlayer>();
                    Assert.IsNotNull(aiLogic, "aiPrefab in GameSetup does not have AIPlayer component");
                    
                    aiLogic.InitPlayer(p, tid);

                    go.GetComponent<NetworkedObject>().Spawn();
                }
            }
        }

        m_hasClientLoaded = true;

        InvokeServerRpc(PlayerLoaded, NetworkingManager.Singleton.LocalClientId, ClientPlayer.Singleton.SteamID);
    }

    void Update()
    {
        isReady = (m_hasClientLoaded && m_hasClientConnected);
    }

    private void OnClientConnected(ulong steamId)
    {
        bool hasConnected = true;

        //ServerState.HandlePlayerConnection(steamId);

        InvokeClientRpcOnClient(ConnectedStatus, steamId, hasConnected);
    }

    [ClientRPC]
    private void ConnectedStatus(bool hasConnected)
    {
        m_hasClientConnected = hasConnected;
    }

    [ServerRPC]
    public void PlayerLoaded(ulong pid, ulong steamID)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);
        GameObject modelObj = Instantiate(PrefabFromTeamID(Match.GetPlayersTeam(steamID)), netObj.transform);
        netObj.GetComponent<Player>().InitilizeModel();

        InvokeClientRpcOnClient(PlayerLoaded, pid);
    }

    [ClientRPC]
    public void PlayerLoaded()
    {
        ClientPlayer.Singleton.State = ServerPlayerState.READY;
        GameManager.Singleton.LocalPlayerInitilized();
    }

    private GameObject PrefabFromTeamID(int teamID)
    {
        if (teamID == 1)
            return bluePrefab;
        else
            return redPrefab;
    }
}
