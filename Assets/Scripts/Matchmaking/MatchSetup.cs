using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
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

    public GameObject loaderPrefab;

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

        SceneManager.sceneLoaded += OnAferSceneLoaded;
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

    private void OnAferSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        GameObject canvas = Instantiate(loadingCanvas);
        loadingScreen = canvas.GetComponent<LoadingScreen>();
        loadingScreen.enabled = true;

        if (scene.name == CONST_GAME_SCENE_NAME)
        {
            HasLoaded = true;

            GameObject go = Instantiate(loaderPrefab);
            MatchLoader loader = go.GetComponent<MatchLoader>();
            loader.Load();

            print("scene loaded");
        }

        loadingScreen.enabled = false;
    }

    private IEnumerator LoadGame()
    {
        while (!HasLoaded)
        {
            yield return null;
        }
    }

}
