using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
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

/// <summary>
/// Handles going from Main Menu scene -> Match scene
/// </summary>
public class MatchSetup : NetworkBehaviour
{
    private const string CONST_GAME_SCENE_NAME = "Match";

    public LoadingScreen currentScreen;
    public GameObject loadingCanvas;
    public LoadingScreen loadingScreen;

    void Start()
    {
        //NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
    }

    public void Setup(CSteamID hostSteamID)
    {
        //NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
        if (!Match.HostServer)
            NetworkManager.Singleton.StartClient();
            

        var prog = NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
        //var operation = SceneManager.LoadSceneAsync(CONST_GAME_SCENE_NAME);
        OnSceneSwitchStarted(prog);

        print("match setup successful");
    }

    private void OnSceneSwitchStarted(SceneSwitchProgress operation)
    {
        StartCoroutine(LoadGame(operation));
    }

    private IEnumerator LoadGame(SceneSwitchProgress operation)
    {
        GameObject canvas = Instantiate(loadingCanvas);
        loadingScreen = canvas.GetComponent<LoadingScreen>();
        loadingScreen.enabled = true;

        while (!operation.IsCompleted)
        {
            yield return null;
        }
    }

}
