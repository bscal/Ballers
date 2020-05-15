using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : MonoBehaviour
{

    public GameObject contestTracker;

    private bool m_showTrackers = true;
    private Player m_player;

    private void Awake()
    {
    }

    void Start()
    {
        GameManager.Singleton.GameStarted += OnGameStarted;
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (!GameManager.Singleton.HasStarted) return;
        if (m_showTrackers)
        {
            contestTracker.transform.position = m_player.transform.position + m_player.transform.forward;
            //contestTracker.transform.Rotate(m_player.transform.forward);
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
