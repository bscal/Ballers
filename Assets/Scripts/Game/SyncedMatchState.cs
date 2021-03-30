using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncedMatchStateData : INetworkSerializable
{

    public bool HasStarted;
    public int TeamWithPossession;
    public ulong PlayerWithBall;
    public TeamData[] Teams = new TeamData[2];

    public SyncedMatchStateData()
    {
        Teams[(int)TeamType.HOME] = GameManager.Singleton.teams[(int)TeamType.HOME].TeamData;
        Teams[(int)TeamType.AWAY] = GameManager.Singleton.teams[(int)TeamType.AWAY].TeamData;
    }

    public void Read(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            HasStarted = reader.ReadBit();
            reader.SkipPadBits();
            TeamWithPossession = reader.ReadInt32Packed();
            PlayerWithBall = reader.ReadUInt64Packed();
            foreach (TeamData team in Teams)
            {
                team.Read(stream);
            }
        }
    }

    public void Write(Stream stream)
    {
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteBit(HasStarted);
            writer.WritePadBits();
            writer.WriteInt32Packed(TeamWithPossession);
            writer.WriteUInt64Packed(PlayerWithBall);

            foreach (TeamData team in Teams)
                team.Write(stream);
        }
    }


    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref HasStarted);
        serializer.Serialize(ref TeamWithPossession);
        serializer.Serialize(ref PlayerWithBall);
        Teams[0].NetworkSerialize(serializer);
        Teams[1].NetworkSerialize(serializer);
    }
}

public class SyncedMatchState : NetworkBehaviour
{
    private SyncedMatchStateData m_state;
    public SyncedMatchStateData State { get { return m_state; } }

    private float m_timerSync;
    private float m_lastSync;

    private void Awake()
    {
        m_state = new SyncedMatchStateData();
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
                m_state.PlayerWithBall = (GameManager.Singleton.BallHandler) ? GameManager.Singleton.BallHandler.OwnerClientId : 0;
                m_state.TeamWithPossession = GameManager.Singleton.Possession;
                m_state.Teams[(int)TeamType.HOME] = GameManager.Singleton.teams[(int)TeamType.HOME].TeamData;
                m_state.Teams[(int)TeamType.AWAY] = GameManager.Singleton.teams[(int)TeamType.AWAY].TeamData;


//                 using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
//                 {
//                     using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
//                     {
//                         foreach (Player p in GameManager.GetPlayers())
//                         {
//                             writer.WriteBool(p.isInsideThree);
//                             writer.WriteBool(p.isInbounds);
//                             writer.WriteBool(p == GameManager.Singleton.BallHandler);
//                         }
//                         // Sends the stream of player dota to all players
//                         InvokeClientRpcOnEveryonePerformance(ReadPlayerFromServer, stream);
//                     }
//                 }
                
                // Syncs the MatchState with all players


                SyncMatchStateClientRpc(m_lastSync, m_state);
            }
        }
    }

    [ClientRpc]
    public void SyncMatchStateClientRpc(float lastSync, SyncedMatchStateData state)
    {
        GameManager.Singleton.SyncState(state);
    }

    [ClientRpc]
    public void ReadPlayerFromServerClientRpc(ulong netID)
    {
        //GameManager.GetPlayerByNetworkID(netID)?.ReadPlayerFromServer(stream);
    }

}
