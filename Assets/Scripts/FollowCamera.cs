﻿using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : NetworkedBehaviour
{
    public NetworkedObject target;

    private Vector3 offset;

    public override void NetworkStart()
    {
        // Sets the camera behind the player with offset.
        target = SpawnManager.GetLocalPlayerObject();
        Vector3 pos = target.transform.position;
        pos.y = 6;
        pos.z = 12;
        transform.position = pos;
        offset = target.transform.position - transform.position;
    }

    void LateUpdate()
    {
        if (isActiveAndEnabled)
        {
            float desiredAngle = target.transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
            transform.position = target.transform.position - (rotation * offset);
            transform.LookAt(target.transform);
        }
    }
}