using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetup : NetworkedBehaviour
{
    private const string DEFAULT_LOADING_MSG = "Loading...";
    private const string NETWORK_LOADING_MSG = "Logging you in...";

    public bool hasClientLoaded = false;
    public GameObject playerPrefab;

    private GameObject m_loadingScreen;
    private Image m_image;
    private Text m_text;
    private bool m_nothingToLoad = false;

    private void Start()
    {
        m_loadingScreen = GameObject.Find("Loading Screen");

        if (!m_loadingScreen)
        {
            enabled = false;
            return;
        }

        m_image = m_loadingScreen.GetComponent<Image>();
        m_image.enabled = true;

        m_text = m_loadingScreen.GetComponentInChildren<Text>();

        if (MatchGlobals.HostServer)
            MatchGlobals.NetworkLobby.HostServer();
        else
            MatchGlobals.NetworkLobby.Connect();

        StartCoroutine(LoadCoroutine());
    }

    void Update()
    {
        if (m_nothingToLoad) return;

        if (!hasClientLoaded)
        {
            SetLoadingText(NETWORK_LOADING_MSG);
        }
        else
        {
            SetLoadingText(DEFAULT_LOADING_MSG);
        }

        m_loadingScreen.SetActive(false);
        m_nothingToLoad = true;
    }

    public void SetLoadingText(string text)
    {
        m_text.text = text;
    }

    [ServerRPC]
    public void PlayerLoaded(ulong pid)
    {
        GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkedObject no = go.GetComponent<NetworkedObject>();
        no.SpawnAsPlayerObject(pid, null, false);

        InvokeClientRpcOnClient(PlayerLoaded, pid);
    }

    [ClientRPC]
    public void PlayerLoaded()
    {
        MatchGlobals.HasLoadedGame = true;
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
