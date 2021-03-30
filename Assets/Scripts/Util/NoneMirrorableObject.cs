using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoneMirrorableObject : MonoBehaviour
{

    public GameObject obj;
    public GameObject other;

    private Animator m_animator;

    private void Start()
    {
        m_animator = NetworkSpawnManager.GetLocalPlayerObject().GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Mirror"))
        {
            transform.position = other.transform.position;
        }
        else
        {
            transform.position = obj.transform.position;
        }
    }
}
