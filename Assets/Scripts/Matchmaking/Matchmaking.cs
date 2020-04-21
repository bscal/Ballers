using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchmaking : MonoBehaviour
{
    public bool IsFinding { get; set; } = false;

    private CallResult<LobbyMatchList_t> m_CallResultLobbyMatchList = new CallResult<LobbyMatchList_t>();

    private float m_timer = 0f;

    void Update()
    {
        if (!IsFinding) return;

        m_timer += Time.deltaTime;

        if (m_timer > 5.0f)
        {
            m_timer = 0f;

            UpdateFinding();
        }
    }

    public void StartFinding()
    {
        IsFinding = true;

        SteamAPICall_t result = SteamMatchmaking.RequestLobbyList();
        m_CallResultLobbyMatchList.Set(result, OnLobbyMatchList);
    }

    private void OnLobbyMatchList(LobbyMatchList_t lobbyMatchList, bool bIOfailure)
    {
        print(lobbyMatchList.m_nLobbiesMatching);
    }


    public void StopFinding()
    {
        IsFinding = false;
    }

    private void UpdateFinding()
    {

    }
}
