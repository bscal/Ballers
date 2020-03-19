using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    private const string DEFAULT_LOADING_MSG = "Loading...";
    private const string NETWORK_LOADING_MSG = "Logging you in...";

    public bool clientLoading = false;

    private GameObject m_loadingScreen;
    private Text m_text;

    private NetworkClientToBackend m_clientToBackend;

    private void Start()
    {

        m_loadingScreen = GameObject.Find("Loading Screen");
        m_loadingScreen.SetActive(true);

        m_text = m_loadingScreen.GetComponentInChildren<Text>();

        m_clientToBackend = GameObject.Find("NetworkClient").GetComponent<NetworkClientToBackend>();

        if (SteamManager.Initialized)
        {
            StartCoroutine(m_clientToBackend.Load(this));
        }
    }

    void Update()
    {
        if (clientLoading)
        {
            m_loadingScreen.SetActive(true);
        }
        else if (m_loadingScreen.activeSelf)
        {
            m_loadingScreen.SetActive(false);
        }

        if (m_loadingScreen.activeSelf)
        {
            if (clientLoading)
            {
                SetLoadingText(NETWORK_LOADING_MSG);
            }
            else
            {
                SetLoadingText(DEFAULT_LOADING_MSG);
            }
        }
    }

    public void SetLoadingText(string text)
    {
        m_text.text = text;
    }
}
