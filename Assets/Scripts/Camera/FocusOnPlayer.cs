using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOnPlayer : MonoBehaviour
{
    public bool startDisabled = true;
    public Vector3 offset;
    public float rotation;

    private GameObject m_target;

    void Start()
    {
        if (NetworkingManager.Singleton == null)
        {
            m_target = GameObject.Find("Player");
        }
        else
        {
            m_target = SpawnManager.GetLocalPlayerObject().gameObject;
        }
        if (startDisabled)
            gameObject.SetActive(false);
        transform.Rotate(rotation, 0, 0);
    }

    void Update()
    {
        if (m_target == null)
            m_target = SpawnManager.GetLocalPlayerObject().gameObject;

        transform.position = m_target.transform.position + offset;
    }
}
