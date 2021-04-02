using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonPlayer : NetworkBehaviour
{

    public bool hasEnteredGame;

    public void Awake()
    {
        DontDestroyOnLoad(this);
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;
    }

    public void PlayerEnteredGame()
    {
        GameManager.Singleton.GameStartedClient += OnGameStarted;
    }

    protected virtual void OnGameStarted()
    {
        hasEnteredGame = true;
    }

    protected void OnSceneSwitched()
    {
        if (IsServer || IsHost)
        {
            PlayerEnteredGame();
            GameManager.Singleton.RegisterPlayer(this.NetworkObject);
        }
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
            sp.steamId = steamId;
            sp.cid = cid;

            Match.SetupPlayer(clientId, steamId, cid);

            sp.state = ServerPlayerState.IDLE;
            sp.hasBeenSetup = true;
        }
    }

    [ClientRpc]
    public void RequestIdsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("RequestIds got");
        ulong steam = ClientPlayer.Singleton.SteamID;
        int cid = ClientPlayer.Singleton.CharData.cid;
        SendIdsServerRpc(steam, cid);
    }
}
