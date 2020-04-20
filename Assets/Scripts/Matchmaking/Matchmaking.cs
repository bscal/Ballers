using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchmaking : MonoBehaviour
{
    public bool IsFinding { get; set; } = false;

    private float m_timer = 0f;

    void Update()
    {
        if (!IsFinding) return;

        m_timer += Time.deltaTime;

        if (m_timer > 5.0f)
        {
            m_timer = 0f;

            UpdateFinding();
        }
    }

    public void StartFinding()
    {
        IsFinding = true;

    }

    public void StopFinding()
    {
        IsFinding = false;
    }

    private void UpdateFinding()
    {

    }
}
