using Ballers;
using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{
    public const float BASE_TARGET = 3.0f;

    public static float MAX_TARGET_HEIGHT;
    public static float BASE_TARGET_HEIGHT;
    public static float TARGET_OFFSET;

    public GameObject meter;
    public RawImage background;
    public RawImage fill;
    public Image target;
    public RawImage glow;

    private RectTransform m_meterTransform;
    private ShotBarData m_shotBarData;
    private float m_timer;
    private bool m_isShooting;

    void Start()
    {
        if (!GetComponent<NetworkedObject>().IsLocalPlayer)
        {
            enabled = false;
            return;
        }

        meter = GameObject.Find("ShotMeter");
        if (meter != null)
        {
            m_meterTransform = meter.GetComponent<RectTransform>();
            background  = GameObject.Find("ShotMeterBG").GetComponent<RawImage>();
            fill        = GameObject.Find("ShotMeterBar").GetComponent<RawImage>();
            target      = GameObject.Find("TargetLine").GetComponent<Image>();
            glow        = GameObject.Find("Glow").GetComponent<RawImage>();
        }

        // These can be local because ShotMeter script only effects local clients.
        //SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Shoot += OnShoot;
        SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Release += OnRelease;

        MAX_TARGET_HEIGHT = background.rectTransform.GetHeight();
        BASE_TARGET_HEIGHT = MAX_TARGET_HEIGHT * 0.8f;
        TARGET_OFFSET = m_meterTransform.GetHeight() / 2.0f;
        meter.SetActive(false);
    }

    private void Update()
    {
        if (m_isShooting)
        {
            m_timer += m_shotBarData.speed * Time.deltaTime;

            m_meterTransform.position = PlayerSettings.Singleton.Current.WorldToScreenPoint(GameManager.GetPlayer().transform.position) - Vector3.left * 64;

            fill.rectTransform.SetHeight(m_timer);

            // Overflowed bar -> Failed shot + disable
            if (fill.rectTransform.GetHeight() >= MAX_TARGET_HEIGHT)
            {
                StopShooting();
                StartCoroutine(Hide(1.0f));
                print("failed");
            }
        }

    }

    public void OnShoot(Player p, ShotData shotData, ShotBarData shotBarData)
    {
        Reset();
        m_shotBarData = shotBarData;

        fill.rectTransform.SetHeight(0.0f);
        target.rectTransform.SetHeight(m_shotBarData.targetSize);
        target.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.targetHeight - m_shotBarData.targetSize / 2);

        meter.SetActive(true);
        m_isShooting = true;
        if (shotBarData.targetFadeSpd != 0f)
            LeanTween.alpha(target.rectTransform, 0f, shotBarData.targetFadeSpd);
        //StartCoroutine(ShootingTimeout());
    }

    public void OnRelease(Player player)
    {
        float dist = Mathf.Abs(m_shotBarData.FinalTargetHeight - m_timer);
        if (dist < m_shotBarData.PerfectLength)
        {
            glow.gameObject.SetActive(true);
            print("perfect");
        }
        else if (dist < m_shotBarData.GoodLength)
        {
            print("good");
        }
        StopShooting();
        StartCoroutine(Hide(3.0f));
    }

    public Vector3 GetBarPosition(float height)
    {
        Vector3 pos = Vector3.zero;
        pos.y = height;
        return pos;
    }

    IEnumerator Hide(float t_wait)
    {
        yield return new WaitForSeconds(t_wait);
        Reset();
    }

    IEnumerator ShootingTimeout()
    {
        yield return new WaitForSeconds(3.0f);
        StopShooting();
    }

    public void StopShooting()
    {
        m_isShooting = false;
    }

    private void Reset()
    {
        fill.rectTransform.SetHeight(0);
        meter.SetActive(false);
        glow.gameObject.SetActive(false);
        m_timer = 0.0f;
    }
}
