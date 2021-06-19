using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallersClient : NetworkBehaviour
{

    public override void NetworkStart()
    {
        if (IsClient)
        {
            ClientPlayer.Instance.localBallersClient = this;
        }
    }

    [ClientRpc]
    public void SyncMatchClientRpc(TeamData home, TeamData away, ClientRpcParams cParams = default)
    {
        Match.matchTeams[0].teamData = home;
        Match.matchTeams[1].teamData = away;
    }

    public void EnteredGame()
    {
        ClientPlayer.Instance.areLocalCamerasEnabled = true;
    }
}
