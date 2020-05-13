using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build.Content;
using UnityEngine;

public class SyncedMatchStateData : IBitWritable
{
    public bool hasStarted { get; set; }
    public int teamWithPossession { get; set; }
    public ulong playerWithBall { get; set; }
    public Team[] teams { get; set; } = new Team[2];

    public SyncedMatchStateData()
    {
        teams[0] = new Team((int)TeamType.HOME, 5);
        teams[1] = new Team((int)TeamType.AWAY, 5);
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            hasStarted = reader.ReadBit();
            reader.SkipPadBits();
            teamWithPossession = reader.ReadInt32Packed();
            playerWithBall = reader.ReadUInt64Packed();
            foreach (Team t in teams)
            {
                t.Read(stream);
            }
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteBit(hasStarted);
            writer.WritePadBits();
            writer.WriteInt32Packed(teamWithPossession);
            writer.WriteUInt64Packed(playerWithBall);
            Debug.Log(teams);
            foreach (Team t in teams)
                t.Write(stream);
        }
    }
}

public class SyncedMatchState : NetworkedBehaviour
{
    private SyncedMatchStateData m_state;
    public SyncedMatchStateData State { get { return m_state; } }

    private GameManager m_gm;

    private float m_timerSync;
    private float m_lastSync;

    private void Awake()
    {
        m_state = new SyncedMatchStateData();
    }

    void Start()
    {
        m_gm = GameManager.Singleton;
    }

    void Update()
    {
        if (IsServer && GameManager.Singleton.HasStarted)
        {
            m_timerSync += Time.deltaTime;
            if (m_timerSync > m_lastSync)
            {
                m_lastSync = m_timerSync + 500;

                m_state.hasStarted = m_gm.HasStarted;
                m_state.playerWithBall = (m_gm.BallHandler) ? m_gm.BallHandler.OwnerClientId : 0;
                m_state.teamWithPossession = m_gm.Possession;
                m_state.teams[(int)TeamType.HOME] = m_gm.teams[(int)TeamType.HOME];
                m_state.teams[(int)TeamType.AWAY] = m_gm.teams[(int)TeamType.AWAY];


                using (PooledBitStream stream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                    {
                        foreach (Player p in GameManager.GetPlayers())
                        {
                            // Writes data for a player that needs to be validated
                            writer.WriteBit(p == GameManager.Singleton.BallHandler);
                            writer.WriteBit(p.isInsideThree);
                        }
                        // Sends the stream of player dota to all players
                        print("hello???");
                        InvokeClientRpcOnEveryonePerformance(ReadPlayerFromServer, stream);
                    }
                }
                // Syncs the MatchState with all players
                InvokeClientRpcOnEveryone(SyncMatchState, m_lastSync, m_state);
            }
        }
    }

    [ClientRPC]
    public void SyncMatchState(float lastSync, SyncedMatchStateData state)
    {
        m_gm.SyncState(state);
    }

    [ClientRPC]
    public void ReadPlayerFromServer(ulong clientid, Stream stream)
    {
        print("streamed");
        print(stream.ToString());
        GameManager.GetPlayer(clientid)?.ReadPlayerFromServer(stream);
    }

}