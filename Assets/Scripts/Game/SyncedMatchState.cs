using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncedMatchStateData : IBitWritable
{
    public bool HasStarted { get; set; }
    public int teamWithPossession { get; set; }
    public ulong playerWithBall { get; set; }
    public TeamData[] teams { get; set; } = new TeamData[2];

    public SyncedMatchStateData()
    {
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            HasStarted = reader.ReadBit();
            reader.SkipPadBits();
            teamWithPossession = reader.ReadInt32Packed();
            playerWithBall = reader.ReadUInt64Packed();
            foreach (TeamData team in teams)
            {
                team.Read(stream);
            }
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteBit(HasStarted);
            writer.WritePadBits();
            writer.WriteInt32Packed(teamWithPossession);
            writer.WriteUInt64Packed(playerWithBall);

            foreach (TeamData team in teams)
                team.Write(stream);
        }
    }
}

public class SyncedMatchState : NetworkedBehaviour
{
    private SyncedMatchStateData m_state;
    public SyncedMatchStateData State { get { return m_state; } }

    private float m_timerSync;
    private float m_lastSync;

    private void Awake()
    {
        m_state = new SyncedMatchStateData();
    }

    void Start()
    {
    }

    void Update()
    {
        if (IsServer && GameManager.Singleton.HasStarted)
        {
            m_timerSync += Time.deltaTime;
            if (m_timerSync > m_lastSync)
            {
                m_lastSync = m_timerSync + 500;

                m_state.HasStarted = GameManager.Singleton.HasStarted;
                m_state.playerWithBall = (GameManager.Singleton.BallHandler) ? GameManager.Singleton.BallHandler.OwnerClientId : 0;
                m_state.teamWithPossession = GameManager.Singleton.Possession;
                m_state.teams[(int)TeamType.HOME] = GameManager.Singleton.teams[(int)TeamType.HOME].TeamData;
                m_state.teams[(int)TeamType.AWAY] = GameManager.Singleton.teams[(int)TeamType.AWAY].TeamData;


                using (PooledBitStream stream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                    {
                        foreach (Player p in GameManager.GetPlayers())
                        {
                            Debug.Log($"NetUpdate for nid: {p.NetworkId}");
                            // Writes data for a player that needs to be validated
                            writer.WriteBit(p == GameManager.Singleton.BallHandler);
                            writer.WriteBit(p.isInsideThree);
                        }
                        // Sends the stream of player dota to all players
                        InvokeClientRpcOnEveryonePerformance(ReadPlayerFromServer, stream);
                        stream.Dispose();
                    }
                }
                // Syncs the MatchState with all players
                InvokeClientRpcOnEveryoneExcept(SyncMatchState, OwnerClientId, m_lastSync, m_state);
            }
        }
    }

    [ClientRPC]
    public void SyncMatchState(float lastSync, SyncedMatchStateData state)
    {
        GameManager.Singleton.SyncState(state);
    }

    [ClientRPC]
    public void ReadPlayerFromServer(ulong clientid, Stream stream)
    {
        GameManager.GetPlayer(clientid)?.ReadPlayerFromServer(stream);
        stream.Dispose();
    }

}