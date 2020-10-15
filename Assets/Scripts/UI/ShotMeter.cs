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

    public static float MAX_TARGET_HEIGHT { get; private set; }
    public static float BASE_TARGET_HEIGHT { get; private set; }
    public static float TARGET_OFFSET { get; private set; }

    public GameObject meter;
    public RawImage background;
    public RawImage fill;
    public Image target;
    public Image targetGood;
    public Image targetOk;
    public RawImage glow;

    private static readonly Color BAD_COLOR = new Color(1, 0, 0, 1);
    private static readonly Color OK_COLOR = new Color(1, 171f / 255f, 35f / 255f, 1);
    private static readonly Color GOOD_COLOR = new Color(1, 1, 0, 1);
    private static readonly Color PERFECT_COLOR = new Color(51f / 255f, 204f / 255f, 51f / 255f, 1);

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
            targetGood  = GameObject.Find("TargetLine_GOOD").GetComponent<Image>();
            targetOk    = GameObject.Find("TargetLine_OK").GetComponent<Image>();
            glow        = GameObject.Find("Glow").GetComponent<RawImage>();
        }

        // These can be local because ShotMeter script only effects local clients.
        //SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Shoot += OnShoot;
        SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Release += OnRelease;

        MAX_TARGET_HEIGHT = background.rectTransform.GetHeight();
        BASE_TARGET_HEIGHT = MAX_TARGET_HEIGHT * 0.65f;
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
        target.rectTransform.SetHeight(m_shotBarData.PerfectLength);
        target.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.targetHeight - m_shotBarData.PerfectLength / 2);

        targetGood.rectTransform.SetHeight(m_shotBarData.GoodLength);
        targetGood.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.targetHeight - m_shotBarData.GoodLength / 2);

        targetOk.rectTransform.SetHeight(m_shotBarData.OkLength);
        targetOk.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.targetHeight - m_shotBarData.OkLength / 2);

        meter.SetActive(true);
        m_isShooting = true;

        if (shotBarData.targetFadeSpd != 0f)
            LeanTween.alpha(target.rectTransform, 0f, shotBarData.targetFadeSpd);
    }

    private void OnRelease(float dist, float diff)
    {
        int grade = m_shotBarData.GetShotGrade(dist);
        if (grade == ShotBarData.GRADE_PERFECT)
            SetColors(PERFECT_COLOR);
        else if (grade == ShotBarData.GRADE_GOOD)
            SetColors(GOOD_COLOR);
        else if (grade == ShotBarData.GRADE_OK)
            SetColors(OK_COLOR);
        else
            SetColors(BAD_COLOR);

        glow.gameObject.SetActive(true);
        StopShooting();
        StartCoroutine(Hide(3.0f));
    }

    private void SetColors(Color color)
    {
        fill.color = color;
        glow.color = color;
    }

    private Vector3 GetBarPosition(float height)
    {
        Vector3 pos = Vector3.zero;
        pos.y = height;
        return pos;
    }

    private IEnumerator Hide(float t_wait)
    {
        yield return new WaitForSeconds(t_wait);
        if (!m_isShooting)
            Reset();
    }

    private void StopShooting()
    {
        m_isShooting = false;
    }

    private void Reset()
    {
        fill.color = Color.white;
        fill.rectTransform.SetHeight(0);
        meter.SetActive(false);
        glow.gameObject.SetActive(false);
        m_timer = 0.0f;
    }
}
