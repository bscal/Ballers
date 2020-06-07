using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderDummy : BasicDummy
{

    private NetworkedObject m_target;

    protected new void Start()
    {
        base.Start();
        GameManager.Singleton.GameStarted += OnGameStarted;
    }

    private void OnGameStarted()
    {
        m_player.Assignment = GameManager.GetPlayer();
    }

    void Update()
    {
        if (!GameManager.Singleton.HasStarted) return;
        m_target = SpawnManager.GetLocalPlayerObject();
        if (m_target == null) return;
        transform.position = m_target.gameObject.transform.position + (m_target.gameObject.transform.forward * 6);
        transform.LookAt(m_target.gameObject.transform);
    }
}
