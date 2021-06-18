using MLAPI;
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
        m_clientPlayer = GameObject.Find("ClientPlayer").GetComponent<ClientPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryPassBall(Player passer, int playerSlot, PassType type)
    {
        
        Player target = Match.matchTeams[passer.props.teamID].GetPlayerBySlot(playerSlot);
        //TryPassBall(passer, target, type);
    }

}
