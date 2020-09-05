using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{
    private const float BASE_TARGET = 3.0f;

    private static float MAX_TARGET_HEIGHT;
    private static float BASE_TARGET_HEIGHT;
    private static float TARGET_OFFSET;

    public float TargetHeight { get; set; } = 0;

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

    public void OnShoot(Player p, ShotBarData shotBarData)
    {
        Reset();
        m_shotBarData = shotBarData;
        m_meterTransform.position = Camera.current.WorldToScreenPoint(p.transform.position) - Vector3.left * 64;

        float targetSize = (MAX_TARGET_HEIGHT * shotBarData.BonusHeight) + BASE_TARGET;
        TargetHeight = (BASE_TARGET_HEIGHT + shotBarData.endOffset) - (targetSize / 2) - TARGET_OFFSET;

        fill.rectTransform.SetHeight(0.0f);
        target.rectTransform.SetHeight(targetSize);
        target.transform.localPosition = GetBarPosition(TargetHeight);

        meter.SetActive(true);
        m_isShooting = true;
        //StartCoroutine(ShootingTimeout());
    }

    public void OnRelease(Player player)
    {
        float dist = Mathf.Abs(TargetHeight - m_timer + m_shotBarData.endOffset - m_shotBarData.startOffset);
        if (dist < 2)
        {
            glow.gameObject.SetActive(true);
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
        TargetHeight = 0;
    }
}
