using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPlayer : NetworkBehaviour
{

    public ulong id;
    public bool hasEnteredGame;

    protected float m_timer;

    public void Awake()
    {
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;
    }

    public override void NetworkStart()
    {
    }

    protected virtual void PlayerEnteredGame()
    {
        GameManager.Singleton.GameStartedClient += OnGameStarted;
    }

    protected virtual void OnGameStarted()
    {
        hasEnteredGame = true;
    }

    protected void OnSceneSwitched()
    {
        PlayerEnteredGame();
    }


    [ServerRpc]
    public void ClientLoadedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ServerPlayer sp = ServerManager.Singleton.players[serverRpcParams.Receive.SenderClientId];
        if (sp != null)
        {
            sp.state = ServerPlayerState.READY;
        }
    }

    [ServerRpc]
    public void SendIdsServerRpc(ulong steamId, int cid, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"Ids got : {clientId} | {steamId} | {cid}");
        if (ServerManager.Singleton.players.TryGetValue(clientId, out ServerPlayer sp))
        {
            id = clientId;
            sp.steamId = steamId;
            sp.cid = cid;

            Match.SetupPlayer(clientId, steamId, cid);

            sp.state = ServerPlayerState.IDLE;
            sp.hasBeenSetup = true;
        }
    }

    public void RequestIdsClient()
    {
        Debug.Log("RequestIds got");
        ulong steam = ClientPlayer.Singleton.SteamID;
        int cid = ClientPlayer.Singleton.CharData.cid;
        id = OwnerClientId;
        SendIdsServerRpc(steam, cid);
    }
}
