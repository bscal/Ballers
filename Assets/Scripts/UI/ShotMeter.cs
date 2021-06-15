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
    public const float BAR_OFFSET = 3.0f;

    public const float MAX_TARGET_HEIGHT = 177;
    public const float BASE_TARGET_HEIGHT = MAX_TARGET_HEIGHT * 0.65f;

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

    private Player m_player;
    private RectTransform m_meterTransform;
    private ShotBarData m_shotBarData;
    private float m_rectFillTimer;
    private float m_hideMeterTimer;

    void Start()
    {
        // This is purely ui for the Local player.
        // Shot bar quality is handle serverside in ShotManager.
        m_player = GameManager.GetPlayer();
        if (m_player.IsOwner)
        {
            meter = GameObject.Find("ShotMeter");
            if (meter != null)
            {
                m_meterTransform = meter.GetComponent<RectTransform>();
                background = GameObject.Find("ShotMeterBG").GetComponent<RawImage>();
                fill = GameObject.Find("ShotMeterBar").GetComponent<RawImage>();
                target = GameObject.Find("TargetLine").GetComponent<Image>();
                targetGood = GameObject.Find("TargetLine_GOOD").GetComponent<Image>();
                targetOk = GameObject.Find("TargetLine_OK").GetComponent<Image>();
                glow = GameObject.Find("Glow").GetComponent<RawImage>();
            }
            meter.SetActive(false);
        }
    }

    private void Update()
    {
        if (m_player.props.isShooting)
        {
            // Increments the height for the fill bar.
            m_rectFillTimer += m_shotBarData.speed * Time.deltaTime;
            fill.rectTransform.SetHeight(m_rectFillTimer);

            // Moves the meter to offset next to the local player.
            m_meterTransform.position = PlayerSettings.Singleton.Current.WorldToScreenPoint(GameManager.GetPlayer().transform.position) - Vector3.left * 64;

            // If we have gone over the max height auto fail shot.
            if (fill.rectTransform.GetHeight() >= MAX_TARGET_HEIGHT)
            {
                print("failed");
            }
        }

        // This just hide the meter if have not shot the ball in X time.
        m_hideMeterTimer -= Time.deltaTime;
        if (m_hideMeterTimer < 0)
        {
            Reset();
        }
    }

    public void OnShoot(Player p, ShotData shotData, ShotBarData shotBarData)
    {
        Reset();
        m_shotBarData = shotBarData;

        fill.rectTransform.SetHeight(0.0f);

        target.rectTransform.SetHeight(m_shotBarData.PerfectLength);
        target.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.FinalTargetHeight - m_shotBarData.PerfectLength / 2);

        targetGood.rectTransform.SetHeight(m_shotBarData.GoodLength);
        targetGood.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.FinalTargetHeight - m_shotBarData.GoodLength / 2);

        targetOk.rectTransform.SetHeight(m_shotBarData.OkLength);
        targetOk.rectTransform.anchoredPosition = GetBarPosition(m_shotBarData.FinalTargetHeight - m_shotBarData.OkLength / 2);

        m_hideMeterTimer = MAX_TARGET_HEIGHT / shotBarData.speed + 2f;
        meter.SetActive(true);

        if (shotBarData.targetFadeSpd != 0f)
            LeanTween.alpha(target.rectTransform, 0f, shotBarData.targetFadeSpd);
    }

    public void OnRelease(float dist, float diff)
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

    private void Reset()
    {
        fill.color = Color.white;
        fill.rectTransform.SetHeight(0);
        m_rectFillTimer = 0.0f;
        glow.gameObject.SetActive(false);
        meter.SetActive(false);
    }
}
