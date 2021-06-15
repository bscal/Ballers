using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallersClient : NetworkBehaviour
{

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
        if (IsClient)
        {
            ClientPlayer.Singleton.ballersClient = this;
        }
    }

    [ClientRpc]
    public void SyncMatchClientRpc(TeamData home, TeamData away, ClientRpcParams cParams = default)
    {
        Match.matchTeams[0].teamData = home;
        Match.matchTeams[1].teamData = away;
    }
}
