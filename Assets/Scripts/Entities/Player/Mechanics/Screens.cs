using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScreenType
{
    NORMAL,
    HARD
}

public enum ScreenModifiers
{
    MISSED,
    LIGHT,
    HARD,
    CRITICAL,
    DODGED
}

public class Screens : MonoBehaviour
{

    [SerializeField]
    private Player m_player;
    [SerializeField]
    private GameObject m_area;

    private bool m_in;
    private float m_duration;

    void Start()
    {
        if (m_player.isDummy && m_area != null)
        {
            m_area.SetActive(true);
        }
    }

    void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.gameObject != gameObject && other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Dummy"))
            {
                if (m_player.isScreening) {
                    float dist = Vector3.Distance(gameObject.transform.position, other.gameObject.transform.position);
                    m_duration = 0;
                    m_in = true;
                    print("screened: " + dist);
                    StartCoroutine(TriggerCount());
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.gameObject != gameObject && other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Dummy"))
            {
                float dist = Vector3.Distance(gameObject.transform.position, other.gameObject.transform.position);
                m_duration = 0;
                m_in = false;
                print("screen out");
            }
        }
    }

    private IEnumerator TriggerCount()
    {
        float t = 0;
        while (m_in)
        {
            t += Time.deltaTime;
            yield return null;
        }
        print(t);
    }
}
