using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderDummy : BasicDummy
{

    protected new void Start()
    {
        GameManager.Singleton.GameStarted += OnGameStarted;
        base.Start();
    }

    private void OnGameStarted()
    {
        m_player.Assignment = GameManager.GetPlayer();
    }

    void Update()
    {
        if (!GameManager.Singleton.HasStarted) return;
        transform.position = m_player.Assignment.transform.position + (m_player.Assignment.transform.forward * 6);
        transform.LookAt(m_player.Assignment.transform);
    }
}
