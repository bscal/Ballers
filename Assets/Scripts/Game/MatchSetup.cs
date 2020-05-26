using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameType
{
    ONES,
    THREES,
    FIVES
}

public class MatchSetup : NetworkedBehaviour
{

    private const string CONST_GAME_SCENE_NAME = "SampleScene";

    public LoadingScreen currentScreen;
    public GameObject loadingCanvas;
    public LoadingScreen loadingScreen;

    public bool HasStarted { get; private set; } = false;
    public bool HasLoaded { get; private set; } = false;
    public ulong MatchID { get; private set; } = 1;
    public CSteamID HostID { get; private set; }

    private Dictionary<byte, ulong> m_playerSlots = new Dictionary<byte, ulong>();
    private GameType m_type;
    private NetworkLobby m_networkLobby;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        m_networkLobby = GameObject.Find("NetworkManager").GetComponent<NetworkLobby>();

        NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
    }

    void Update()
    {

    }

    public void Setup(LobbyEnter_t lobbyEnter, ulong hostSteamID)
    {
        HostID = new CSteamID(hostSteamID);
        m_networkLobby.SetSteamIDToConnect(hostSteamID);

        if (m_networkLobby.hostServer) SetupServer();
        else ConnectToServer();

        MatchReady();
        print("match setup successful");
    }

    private void MatchReady()
    {
        HasStarted = true;

        if (NetworkingManager.Singleton.IsServer)
        {
            NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
        }
    }

    private void SetupServer()
    {
        m_networkLobby.HostServer();
    }

    private void ConnectToServer()
    {
        m_networkLobby.Connect();
    }

    private void OnSceneSwitchStarted(AsyncOperation operation)
    {
        StartCoroutine(LoadGame(operation));
    }

    private IEnumerator LoadGame(AsyncOperation operation)
    {
        GameObject canvas = Instantiate(loadingCanvas);
        loadingScreen = canvas.GetComponent<LoadingScreen>();
        loadingScreen.enabled = true;

        while (!operation.isDone)
        {
            yield return null;
        }
    }

}
