using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(GameObject))]
public class RoundShotMeter : MonoBehaviour
{

    private const float MAX_TIME = 1.0f;

    [Header("Meter Components")]
    [SerializeField]
    private GameObject m_meter;
    [SerializeField]
    private RawImage m_background;
    [SerializeField]
    private RawImage m_fill;
    [SerializeField]
    private RectTransform m_meterTransform;

    [Header("Meter Color Values")]
    [Description("Colors go from: Start -> Target -> End")]
    [SerializeField]
    [Tooltip("Color the meter starts on.")]
    private Color m_startColor;
    [SerializeField]
    [Tooltip("Color the meter is highest point.")]
    private Color m_targetColor;
    [SerializeField]
    [Tooltip("Color the meter ends on.")]
    private Color m_endColor;

    private bool m_isActive;
    private float m_timer;
    private Color m_currentColor;

    // Meter data;
    private float m_speed;
    private float m_difficulty;

    /*
     * TODO
     * A smaller faster round shotmeter.
     * I think I will use color to tell what type to use
     * and a static size.
     * 
     * red -> green -> red
     * 
     * will be easier then shotmeter but overall faster
     * some way to gradually from colors
     * 
     * ? should we think about some way to handle ping from player to server?
     */


    void Start()
    {
        Assert.IsTrue(m_meter == null, "Meter GameObject is null!");

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (m_isActive)
        {
            m_timer += Time.deltaTime * m_speed;

            if (m_timer > MAX_TIME)
            {
                // Failed
            }

            // TODO color gradient 
        }
    }

    public void StartMeter(float speed, float difficulty)
    {
        m_speed = speed;
        m_difficulty = difficulty;
        MoveToPlayer();
        m_isActive = true;
    }

    public void StopMeter()
    {
        //TODO do we need any offset for ping?
        Reset();
    }

    private void MoveToPlayer()
    {
        GameObject playerObj = SpawnManager.GetLocalPlayerObject().gameObject;
    }

    private void Reset()
    {
        m_timer = 0;
        m_isActive = false;
        m_currentColor = m_startColor;
        gameObject.SetActive(false);
    }
}
