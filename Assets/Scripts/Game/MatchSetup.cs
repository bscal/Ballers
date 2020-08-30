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
    private const string CONST_GAME_SCENE_NAME = "Match";

    public LoadingScreen currentScreen;
    public GameObject loadingCanvas;
    public LoadingScreen loadingScreen;

    void Start()
    {
        NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
    }

    public void Setup(CSteamID hostSteamID)
    {
        //NetworkSceneManager.SwitchScene(CONST_GAME_SCENE_NAME);
        var operation = SceneManager.LoadSceneAsync(CONST_GAME_SCENE_NAME);
        StartCoroutine("LoadGame", operation);

        print("match setup successful");
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
