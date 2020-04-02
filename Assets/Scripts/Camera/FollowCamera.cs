using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject target;

    private Vector3 offset;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }
    }

    void LateUpdate()
    {
        // Check if player is null
        if (target == null)
        {
            NetworkedObject netPlayer = SpawnManager.GetLocalPlayerObject();
            if (!netPlayer) return;

            target = netPlayer.gameObject;
            SetPositions(); 
        }

        transform.position = target.transform.position - target.transform.forward * 12 + target.transform.up * 8;
        transform.LookAt(target.transform);
    }

    private void SetPositions()
    {
        Vector3 pos = target.transform.position;
        pos.y = 6;
        pos.z = 12;
        transform.position = pos;
        offset = target.transform.position - transform.position;
    }
}