using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screens : MonoBehaviour
{

    [SerializeField]
    private Player m_player;
    [SerializeField]
    private GameObject m_area;

    void Start()
    {
    }

    void Update()
    {
        if (m_player.isDummy && m_area != null)
        {
            m_area.SetActive(true);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            if (other.gameObject != gameObject && other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Dummy"))
            {
                if (m_player.isScreening) {
                    float dist = Vector3.Distance(gameObject.transform.position, other.gameObject.transform.position);
                    print("screened: " + dist);
                }
            }
        }
    }
}
