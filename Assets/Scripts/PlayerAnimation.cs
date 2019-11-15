using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    public GameObject stand;

    public GameObject[] walk;
    public int walkWait = 5;

    public WalkState State { get; set; }

    private int m_waits = 0;
    private int m_lastFrame = 0;
    private int m_frame = 0;

    // Start is called before the first frame update
    void Start()
    {
        State = WalkState.IDLE;
        stand.SetActive(true);
        GameObject.Find("Frame N").SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_waits > walkWait)
        {
            m_waits = 0;
            // Animation Logic goes here
            walk[m_lastFrame].SetActive(false);
            walk[m_frame].SetActive(true);

            // Animation Logic ends here

            m_lastFrame = m_frame;
            m_frame++;

            if (m_frame + 1 > walk.Length)
                m_frame = 0;
        }
        m_waits++;
    }
}

public enum WalkState
{
    NONE,
    IDLE
}
