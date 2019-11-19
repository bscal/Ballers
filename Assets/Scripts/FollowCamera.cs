using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject target;
    Vector3 offset;

    void Start()
    {
        // Sets the camera behind the player with offset.
        Vector3 pos = GameObject.Find("PlayerObject").transform.position;
        pos.y = 6;
        pos.z = 12;
        transform.position = pos;
        offset = target.transform.position - transform.position;
    }

    void LateUpdate()
    {
        float desiredAngle = target.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
        transform.position = target.transform.position - (rotation * offset);
        transform.LookAt(target.transform);
    }
}