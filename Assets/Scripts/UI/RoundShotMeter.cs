using Ballers;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

[RequireComponent(typeof(GameObject))]
public class RoundShotMeter : MonoBehaviour
{

    private const float MAX_TIME = 0.9f;
    private const float TARGET_TIME = 0.6f;

    [Header("Meter Components")]
    [SerializeField]
    private GameObject m_meter;
    private RectTransform m_meterTransform;
    [SerializeField]
    private RawImage m_fill;
    [SerializeField]
    private GameObject m_glow;


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
    private float m_score;

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
        if (m_meter == null) Debug.LogError("Meter GameObject is null!");
        m_meterTransform = m_meter.GetComponent<RectTransform>();

        Reset();
    }

    void Update()
    {
        if (m_isActive)
        {
            m_timer += Time.deltaTime * m_speed;

            if (m_timer < TARGET_TIME)
            {
                m_currentColor = Color.Lerp(m_startColor, m_targetColor, m_timer);
            }
            else
            {
                m_currentColor = Color.Lerp(m_targetColor, m_endColor, m_timer);
            }

            if (m_timer > MAX_TIME)
            {
                m_currentColor = m_endColor;
                LeanTween.delayedCall(2.0f, () => StopMeter(MAX_TIME));
            }

            m_fill.color = m_currentColor;

            m_meterTransform.position = PlayerSettings.Singleton.Current.WorldToScreenPoint(GameManager.GetPlayer().transform.position) - Vector3.left * 64;
        }
    }

    public void StartMeter(float speed, float difficulty)
    {
        m_speed = speed;
        m_difficulty = difficulty;
        m_isActive = true;
        gameObject.SetActive(true);
    }

    public void StopMeter(float score)
    {
        //TODO do we need any offset for ping?
        m_score = score;
        Reset();
    }

    public void Response(float score)
    {
        // Reponse from the server timer -> client
        StopMeter(score);
    }

    public IEnumerator ServerTimer(ulong netID, float speed, float difficulty, Action<ulong, float> callback)
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * speed;
            if (timer > MAX_TIME)
            {
                break;
            }
            yield return null;
        }
        callback.Invoke(netID, MAX_TIME - timer);
    }

    private void Reset()
    {
        m_timer = 0;
        m_isActive = false;
        m_currentColor = m_startColor;
        gameObject.SetActive(false);
        m_glow.SetActive(false);
    }
}
