using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    public GameObject obj;
    public Color color;

    private Player m_player;

    void Start()
    {
        m_player = SpawnManager.GetLocalPlayerObject().GetComponentInChildren<Player>();
        obj.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    private void Update()
    {
        transform.parent.position = m_player.transform.position + new Vector3(0,.2f,0);
    }
}
