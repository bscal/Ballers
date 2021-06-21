using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientNetworkHandler : NetworkBehaviour
{

    public Player player;
    public PlayerControls playerControls;

    protected ClientPlayer m_clientPlayer;
    protected BallController m_ballHandling;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void NetworkStart()
    {
        if (IsLocalPlayer)
        {
            NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;
            m_clientPlayer = GameObject.Find("NetworkClient").GetComponent<ClientPlayer>();
        }
    }

    protected void OnSceneSwitched()
    {
        print("On Scene Switched");
        ClientPlayer.Instance.Initilize(player, this);
        if (IsLocalPlayer)
            SceneChangeServerRpc();
    }

    [ServerRpc]
    public void SceneChangeServerRpc()
    {
        ServerManager.Instance.PlayerEnteredGame(OwnerClientId);
    }

    [ServerRpc]
    public void PlayerLoadedServerRpc()
    {
        ServerManager.Instance.PlayerLoaded(OwnerClientId);
    }

    [ServerRpc]
    public void PlayerReadyUpServerRpc()
    {
        ServerManager.Instance.PlayerReadyUp(OwnerClientId);
    }

    public void CallForBall()
    {
        m_ballHandling.PlayerCallForBallServerRpc(NetworkObjectId);
    }

    public void TryPassBall(Player passer, int playerSlot, PassType type)
    {
        if (IsOwner && passer.props.slot != playerSlot)
        {
            Player target = Match.matchTeams[passer.props.teamID].GetPlayerBySlot(playerSlot);
            m_ballHandling.PassBallServerRpc(passer.NetworkObjectId, target.NetworkObjectId, type);
        }
    }

    [ClientRpc]
    public void PassBallSuccessClientRPC(ClientRpcParams clientRpcParams = default)
    {

    }

    [ClientRpc]
    public void RecievePassClientRpc(ulong targetPid, Vector3 pos, PassType type, ClientRpcParams cParams = default)
    {
        
    }

    public void SetReadyStatus()
    {
        SetReadyStatusServerRpc();
    }

    [ServerRpc]
    private void SetReadyStatusServerRpc()
    {
        ServerManager.Instance.PlayerReadyUp(OwnerClientId);
    }

}
