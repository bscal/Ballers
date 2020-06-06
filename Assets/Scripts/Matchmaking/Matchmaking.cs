using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchmaking : MonoBehaviour
{
    public bool IsFinding { get; set; } = false;
    public bool InLobby { get; set; } = false;

    private CSteamID m_lobbyID = CSteamID.Nil;

    private readonly CallResult<LobbyMatchList_t> m_CallResultLobbyMatchList = new CallResult<LobbyMatchList_t>();
    private readonly CallResult<LobbyCreated_t> m_CallResultLobbyCreated = new CallResult<LobbyCreated_t>();
    private readonly CallResult<LobbyEnter_t> m_CallResultLobbyEnter = new CallResult<LobbyEnter_t>();
    private Callback<LobbyEnter_t> m_CallbackLobbyEnter;

    private float m_timer = 0f;
    private MatchSetup m_matchSetup;

    private void Start()
    {
        m_matchSetup = GameObject.Find("MatchManager").GetComponent<MatchSetup>();
        m_CallbackLobbyEnter = new Callback<LobbyEnter_t>(OnLobbyJoin);
    }

    void Update()
    {
        if (!IsFinding) return;

        m_timer += Time.deltaTime;

        if (m_timer > 5.0f)
        {
            m_timer = 0f;

            UpdateFinding();
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
        SteamMatchmaking.SetLobbyData(lobbyID, "Host", ClientPlayer.Singleton.SteamID.ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, "NeededPlayers", $"{1}");
    }

    // Callback for Matchmaking JoinLobby
    private void OnLobbyJoin(LobbyEnter_t lobbyEnter)
    {
        m_lobbyID = new CSteamID(lobbyEnter.m_ulSteamIDLobby);
        EChatRoomEnterResponse response = (EChatRoomEnterResponse)lobbyEnter.m_EChatRoomEnterResponse;
        print("created lobby waiting to test 3secs...");
        string hostSteamID = SteamMatchmaking.GetLobbyData(m_lobbyID, "Host");
        ulong steamid = ulong.Parse(hostSteamID);

        int neededPlayers = int.Parse(SteamMatchmaking.GetLobbyData(m_lobbyID, "NeededPlayers"));
        int playerCount = SteamMatchmaking.GetNumLobbyMembers(m_lobbyID);

        MatchGlobals.NetworkLobby.SetSteamIDToConnect(steamid);
        if (steamid == ClientPlayer.Singleton.SteamID)
            MatchGlobals.HostServer = true;
        MatchGlobals.MatchSettings = new MatchSettings(BallersGamemode.SP_BOTS, 5, 60.0f * 6.0f, 4);
        MatchGlobals.PlayersNeeded = neededPlayers;
        MatchGlobals.MatchID = 1;

        ClientPlayer.Singleton.State = ServerPlayerState.JOINED;

        // FOR DEBUGGING
        //m_matchSetup.Setup(lobbyEnter, steamid);

        Debug.Log($"{playerCount} / {neededPlayers}");

        if (playerCount >= neededPlayers)
        {
            Debug.Log($"Required players met. Starting...");
            m_matchSetup.Setup(lobbyEnter, steamid);
        }

        StartCoroutine(Test(lobbyEnter.m_ulSteamIDLobby));
    }

    // FOR TESTING
    private IEnumerator Test(ulong l)
    {
        //test if lobby is created and leaves lobby (destroys on steams end)
        yield return new WaitForSeconds(3.0f);
        SteamMatchmaking.AddRequestLobbyListStringFilter("Gamemode", "Ballers-1v1", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamAPICall_t result = SteamMatchmaking.RequestLobbyList();
        m_CallResultLobbyMatchList.Set(result, OnLobbyMatchListTest);
        yield return new WaitForSeconds(1.0f);
        print("leaving lobby...");
        SteamMatchmaking.LeaveLobby(new CSteamID(l));
        m_lobbyID = CSteamID.Nil;
    }

    // FOR TESTING
    private void OnLobbyMatchListTest(LobbyMatchList_t lobbyMatchList, bool bIOfailure)
    {
        uint num = lobbyMatchList.m_nLobbiesMatching;
        print(num);
    }

    public void StopFinding()
    {
        IsFinding = false;
    }

    private void UpdateFinding()
    {

    }
}
