using MLAPI;
using MLAPI.Messaging; 
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

    private bool m_hasClientLoaded = false;
    private bool m_hasClientConnected = false;

    private void Start()
    {
        if (Match.HostServer)
            Match.NetworkLobby.HostServer();
        else
            Match.NetworkLobby.Connect();

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
                    AIPlayer aiLogic = go.GetComponent<AIPlayer>();
                    Assert.IsNotNull(aiLogic, "aiPrefab in GameSetup does not have AIPlayer component");

                    Player p = go.GetComponent<Player>();
                    p.isAI = true;
                    p.TeamID = tid;
                    p.slot = Match.matchTeams[tid].numOfPlayers + i;

                    GameManager.AddAI(aiLogic);

                    go.GetComponent<NetworkedObject>().Spawn();
                }
            }


        }

        m_hasClientLoaded = true;
        print("creating player");
        InvokeServerRpc(PlayerLoaded, NetworkingManager.Singleton.LocalClientId);
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
    public void PlayerLoaded(ulong pid)
    {
        if (Match.HostServer)
        {
            //GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            //NetworkedObject no = go.GetComponent<NetworkedObject>();
            //no.SpawnAsPlayerObject(OwnerClientId);
        }

        InvokeClientRpcOnClient(PlayerLoaded, pid);
    }

    [ClientRPC]
    public void PlayerLoaded()
    {
        ClientPlayer.Singleton.State = ServerPlayerState.READY;
        GameManager.Singleton.LocalPlayerInitilized();
    }

    [ClientRPC]
    public void AllPlayersLoaded()
    {

    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        InvokeServerRpc(PlayerLoaded, NetworkingManager.Singleton.LocalClientId);
    }
}
