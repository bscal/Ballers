using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOnPlayer : MonoBehaviour
{
    public Vector3 offset;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.position = SpawnManager.GetLocalPlayerObject().gameObject.transform.position + offset;
        print("updated");
    }
}
