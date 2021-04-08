using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
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

    private bool m_hasSetup;

    void Awake()
    {
        DontDestroyOnLoad(this);

    }

    internal void SetServerManagerInstance(ServerManager serverManager)
    {
        serverManager.AllPlayersLoaded += OnAllPlayersLoaded;
    }

    public void Setup(CSteamID hostSteamID)
    {
        if (m_hasSetup)
            return;

        m_hasSetup = true;
        NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;

        if (m_lobby.isDedicated)
            return;

        if (!Match.HostServer)
            NetworkManager.Singleton.StartClient();
        else
            ServerManager.Singleton.SetupHost();
    }

    private void OnAllPlayersLoaded()
    {
        print("Switching");
        NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
    }

    private void OnSceneSwitchStarted(AsyncOperation operation)
    {
        print("On Scene Switch Started");
        StartCoroutine(LoadGame(operation));
    }

    private void OnSceneSwitched()
    {
        print("On Scene Switch Switched");
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
