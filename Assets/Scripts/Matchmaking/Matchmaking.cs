using MLAPI;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchmaking : MonoBehaviour
{
    const int ENTER = 0x0001;
    const int LEAVE = 0x0002;
    const int DISCONNECT = 0x0004;

    public bool IsFinding { get; set; } = false;
    public bool InLobby { get; set; } = false;

    private CSteamID m_lobbyID = CSteamID.Nil;

    private readonly CallResult<LobbyMatchList_t> m_CallResultLobbyMatchList = new CallResult<LobbyMatchList_t>();
    private readonly CallResult<LobbyCreated_t> m_CallResultLobbyCreated = new CallResult<LobbyCreated_t>();
    private readonly CallResult<LobbyEnter_t> m_CallResultLobbyEnter = new CallResult<LobbyEnter_t>();
    private Callback<LobbyDataUpdate_t> m_CallResultLobbyChatUpdate;
    private Callback<LobbyEnter_t> m_CallbackLobbyEnter;

    private float m_timer = 0f;
    private MatchSetup m_matchSetup;
    [SerializeField]
    private NetworkLobby m_lobby;

    private void Start()
    {
        if (ServerManager.isDedicatedServer)
            Destroy(this);

        m_matchSetup = GameObject.Find("MatchManager").GetComponent<MatchSetup>();
        m_CallbackLobbyEnter = new Callback<LobbyEnter_t>(OnLobbyJoin);
        m_CallResultLobbyChatUpdate = new Callback<LobbyDataUpdate_t>(OnLobbyChatUpdate);
    }

    void Update()
    {
        m_timer += Time.deltaTime;
        
        if (m_timer > 5.0f)
        {
            if (IsFinding)
            {
                m_timer = 0f;

                UpdateFinding();
                SteamAPI.RunCallbacks();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (m_lobbyID != null)
            SteamMatchmaking.LeaveLobby(m_lobbyID);
    }

    public void StartFinding()
    {
        if (!SteamManager.Initialized) return;
        IsFinding = true;

        // This filters were lobbies with correct gamemodes this is required before RequestLobbyList
        SteamMatchmaking.AddRequestLobbyListStringFilter("Gamemode", "Ballers-1v1", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamAPICall_t result = SteamMatchmaking.RequestLobbyList();
        m_CallResultLobbyMatchList.Set(result, OnLobbyMatchList);
    }

    // Callback for SteamMatchmaking RequestLobbyList
    private void OnLobbyMatchList(LobbyMatchList_t lobbyMatchList, bool bIOfailure)
    {
        uint num = lobbyMatchList.m_nLobbiesMatching;
        // If no lobbies create one
        if (num < 1)
        {
            SteamAPICall_t result = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 2);
            m_CallResultLobbyCreated.Set(result, OnLobbyCreated);
        }
        else
        {
            CSteamID lobby = SteamMatchmaking.GetLobbyByIndex(0);
            SteamAPICall_t result = SteamMatchmaking.JoinLobby(lobby);
        }

        SteamAPI.RunCallbacks();
    }

    // Callback for Matchmaking CreateLobby
    private void OnLobbyCreated(LobbyCreated_t lobbyCreated, bool bIOfailure)
    {
        CSteamID lobbyID = new CSteamID(lobbyCreated.m_ulSteamIDLobby);

        // Sets lobby gamemode
        SteamMatchmaking.SetLobbyData(lobbyID, "Gamemode", "Ballers-1v1");
        // Sets lobby host steamid
        SteamMatchmaking.SetLobbyData(lobbyID, "Host", ClientPlayer.Instance.SteamID.ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, "NeededPlayers", "1");

        if (!m_lobby.usingDedicated)
        {
            Match.NetworkLobby.SetSteamIDToConnect(ClientPlayer.Instance.SteamID);
            Match.HostID = new CSteamID(ClientPlayer.Instance.SteamID);
            Match.HostServer = true;

            NetworkManager.Singleton.StartHost();
        }
    }

    // Callback for Matchmaking JoinLobby
    private void OnLobbyJoin(LobbyEnter_t lobbyEnter)
    {
        m_lobbyID = new CSteamID(lobbyEnter.m_ulSteamIDLobby);
        EChatRoomEnterResponse response = (EChatRoomEnterResponse)lobbyEnter.m_EChatRoomEnterResponse;

        string hostSteamID = SteamMatchmaking.GetLobbyData(m_lobbyID, "Host");
        ulong steamid = ulong.Parse(hostSteamID);

        SteamMatchmaking.SetLobbyMemberData(m_lobbyID, "cid", ClientPlayer.Instance.Cid.ToString());

        Match.InitMatch(new MatchSettings(BallersGamemode.SP_BOTS, 5, 4, 60.0 * 12.0, 24.0));
        Match.NetworkLobby.SetSteamIDToConnect(steamid);
        Match.MatchSettings = new MatchSettings(BallersGamemode.SP_BOTS, 5, 4, 60.0 * 12.0, 24.0);
        Match.PlayersNeeded = int.Parse(SteamMatchmaking.GetLobbyData(m_lobbyID, "NeededPlayers"));
        Match.MatchID = 1;

        ClientPlayer.Instance.State = ServerPlayerState.JOINING;
    }

    private void OnLobbyChatUpdate(LobbyDataUpdate_t lobbyDataUpdate)
    {
        ulong steamid = lobbyDataUpdate.m_ulSteamIDMember;
        if (lobbyDataUpdate.m_bSuccess == 1)
        {
            print("Player joining... " + steamid);
            string pCountStr = SteamMatchmaking.GetLobbyData(m_lobbyID, "NeededPlayers");
            if (string.IsNullOrEmpty(pCountStr))
            {
                Debug.LogWarning("pCountStr is NullOrEmpty");
                return;
            }
            if (!int.TryParse(pCountStr, out int neededPlayers))
            {
                Debug.LogWarning("pCountStr is not an int");
                return;
            }
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(m_lobbyID);

            Debug.Log($"{playerCount} / {neededPlayers}");
            if (playerCount >= neededPlayers)
            {
                Debug.Log($"Required players met. Starting...");
                SteamMatchmaking.LeaveLobby(m_lobbyID);
                m_matchSetup.Setup(Match.HostID);
                StopFinding();
            }
        }
    }

    public void StopFinding()
    {
        IsFinding = false;
    }

    private void UpdateFinding()
    {

    }

}
