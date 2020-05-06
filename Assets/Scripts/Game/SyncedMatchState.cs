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

public struct SyncedMatchStateData : IBitWritable
{
    public bool hasStarted;
    public int teamWithPossession;
    public ulong playerWithBall;
    public Team[] teams;

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            hasStarted = reader.ReadBit();
            reader.SkipPadBits();
            teamWithPossession = reader.ReadInt32Packed();
            playerWithBall = reader.ReadUInt64Packed();
            foreach (Team t in teams)
                t.Read(stream);
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

    public SyncedMatchState()
    {
        m_state = new SyncedMatchStateData();
    }

    void Start()
    {
        m_gm = GameManager.Singleton;
    }

    void Update()
    {
        if (IsServer)
        {
            m_timerSync += Time.deltaTime;
            if (m_timerSync - 500 > Time.time)
            {
                m_lastSync = Time.time;

                m_state.hasStarted = m_gm.HasStarted;
                m_state.playerWithBall = m_gm.BallHandler.OwnerClientId;
                m_state.teamWithPossession = m_gm.Possession;
                m_state.teams[(int)TeamType.HOME] = m_gm.teams[(int)TeamType.HOME];
                m_state.teams[(int)TeamType.AWAY] = m_gm.teams[(int)TeamType.AWAY];

                InvokeClientRpcOnEveryone(SyncMatchState, m_lastSync, m_state);
            }
        }
    }

    void NetworkedStart()
    {

    }

    [ClientRPC]
    public void SyncMatchState(float lastSync, SyncedMatchStateData state)
    {

    }
   
}