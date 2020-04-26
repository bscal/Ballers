using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOnPlayer : MonoBehaviour
{
    public bool startDisabled = true;
    public bool GetLocalPlayerObject = true;
    public Vector3 offset;
    public float rotation;

    private GameObject m_target;

    void Start()
    {
        if (GetLocalPlayerObject)
        {
            m_target = SpawnManager.GetLocalPlayerObject().gameObject;
            Debug.LogError(string.Format("FocusOnPlayer: Target of GameObject: {0} is null? Did you forget to set something?", gameObject.name));
        }
        else
        {
            m_target = GameObject.Find("Player");
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
