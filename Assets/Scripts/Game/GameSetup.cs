using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameSetup handles getting the game ready for play. Making sure players are connected. MatchGlobals are set.
/// </summary>
public class GameSetup : NetworkedBehaviour
{
    private const string DEFAULT_LOADING_MSG = "Loading...";
    private const string NETWORK_LOADING_MSG = "Logging you in...";

    public bool isReady = false;

    public GameObject playerPrefab;

    private GameObject m_loadingScreen;
    private Image m_image;
    private Text m_text;
    private bool m_hasClientLoaded = false;
    private bool m_hasClientConnected = false;

    private void Start()
    {
        m_loadingScreen = GameObject.Find("Loading Screen");

        if (!m_loadingScreen)
        {
            enabled = false;
            return;
        }

        m_image = m_loadingScreen.GetComponent<Image>();
        m_image.enabled = false;

        m_text = m_loadingScreen.GetComponentInChildren<Text>();

        if (MatchGlobals.HostServer)
            MatchGlobals.NetworkLobby.HostServer();
        else
            MatchGlobals.NetworkLobby.Connect();

        if (IsServer)
            NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        m_hasClientLoaded = true;

        StartCoroutine(LoadCoroutine());
    }

    void Update()
    {
        isReady = (m_hasClientLoaded && m_hasClientConnected);
    }

    private void OnClientConnected(ulong id)
    {
        bool hasConnected = true;

        MatchGlobals.HandlePlayerConnection(id);

        InvokeClientRpcOnClient(ConnectedStatus, id, hasConnected);
    }

    [ClientRPC]
    private void ConnectedStatus(bool hasConnected)
    {
        m_hasClientConnected = hasConnected;
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
