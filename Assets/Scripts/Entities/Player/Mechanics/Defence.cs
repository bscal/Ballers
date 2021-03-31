using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : MonoBehaviour
{

    public GameObject contestTracker;

    private bool m_showTrackers = true;
    private Player m_player;
    private Vector3 m_contestLook;

    private void Awake()
    {
    }

    void Start()
    {
        GameManager.Singleton.GameStartedClient += OnGameStarted;
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (!GameManager.Singleton.HasStarted) return;
        if (m_player == null || !m_player.IsOwner) return;
        if (m_showTrackers)
        {
            m_contestLook = m_player.transform.position + m_player.transform.forward;
            contestTracker.transform.position = m_contestLook;

            contestTracker.transform.LookAt(m_contestLook + m_player.transform.forward * 2);
            contestTracker.transform.rotation = Quaternion.Euler(0f, contestTracker.transform.eulerAngles.y, 0f);
        }
    }

    void OnGameStarted()
    {
        m_player = GameManager.GetPlayer();
    }

    public void SetTrackerVisiblity(bool visible)
    {
        m_showTrackers = visible;
        contestTracker.SetActive(visible);
    }
}
