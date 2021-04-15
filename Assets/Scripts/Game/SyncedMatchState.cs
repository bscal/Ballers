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

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref HasStarted);
        serializer.Serialize(ref TeamWithPossession);
        serializer.Serialize(ref PlayerWithBall);
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

                //SyncMatchStateClientRpc(m_lastSync, m_state);
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
