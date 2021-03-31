using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using Steamworks;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles going from Main Menu scene -> Match scene
/// </summary>
public class MatchSetup : MonoBehaviour
{
    private const string CONST_GAME_SCENE_NAME = "Match";

    public LoadingScreen currentScreen;
    public GameObject loadingCanvas;
    public LoadingScreen loadingScreen;

    [SerializeField]
    private NetworkLobby m_lobby;

    void Awake()
    {
        DontDestroyOnLoad(this);
        NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;
        if (!Match.HostServer)
            ServerManager.AllPlayersLoaded += OnAllPlayersLoaded;
    }

    public void Setup(CSteamID hostSteamID)
    {
        if (ServerManager.Singleton.GetStartupState() != StartupState.NONE)
            return;

        if (!Match.HostServer)
            NetworkManager.Singleton.StartClient();
        else if (NetworkManager.Singleton.IsServer)
            ServerManager.Singleton.SetupServer();
    }

    private void OnAllPlayersLoaded()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
        }
    }

    private void OnSceneSwitchStarted(AsyncOperation operation)
    {
        StartCoroutine(LoadGame(operation));
    }

    private void OnSceneSwitched()
    {
        m_lobby.ClientLoadedServerRpc();
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
