using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameType
{
    ONES,
    THREES,
    FIVES
}

public class MatchSetup : MonoBehaviour
{

    private const string GAME_SCENE_NAME = "";

    public bool HasStarted { get; private set; }
    public ulong MatchID { get; private set; }
    public CSteamID HostID { get; private set; }

    private Dictionary<byte, ulong> m_playerSlots = new Dictionary<byte, ulong>();
    private GameType m_type;
    private NetworkLobby m_networkLobby;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void Setup(LobbyEnter_t lobbyEnter, ulong hostSteamID)
    {
        HostID = new CSteamID(hostSteamID);
        m_networkLobby.SetSteamIDToConnect(hostSteamID);
        MatchID = 1;

        if (hostSteamID == ClientPlayer.Singleton.SteamID) SetupServer();
        else ConnectToServer();

        MatchReady();
        print("match setup successful");
    }

    private void MatchReady()
    {
        HasStarted = true;
    }

    private void SetupServer()
    {
        m_networkLobby.HostServer();
    }

    private void ConnectToServer()
    {
        m_networkLobby.Connect();
    }

    private void LoadGameScene()
    {

    }

    private void AfterLoadGameScene()
    {

    }

}
