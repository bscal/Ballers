using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{

    public const float BASE_SPEED = 50.0f;

    float speed = BASE_SPEED;

    public float height;
    public float targetHeight { get; set; }
    public Vector2 position;
    public Vector3 targetPos = Vector3.zero;

    public GameObject meter;
    public RawImage background;
    public RawImage fill;
    public Image target;
    public RawImage glow;
    
    private bool m_isShooting = false;
    private RectTransform m_rectTrans;

    private IEnumerator m_coroutine;

    // Start is called before the first frame update
    void Start()
    {

        if (!GetComponent<NetworkedObject>().IsLocalPlayer)
        {
            enabled = false;
            return;
        }

        if (GameObject.Find("ShotMeter"))
        {
            meter       = GameObject.Find("ShotMeter");
            background  = GameObject.Find("ShotMeterBG").GetComponent<RawImage>();
            fill        = GameObject.Find("ShotMeterBar").GetComponent<RawImage>();
            target      = GameObject.Find("ShotLine").GetComponent<Image>();
            glow        = GameObject.Find("Glow").GetComponent<RawImage>();
            m_rectTrans = fill.rectTransform;
        }

        //SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Shoot += OnShoot;
        SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Release += OnRelease;

        //NetworkEvents.Singleton.RegisterEvent(NetworkEvent.PLAYER_SHOOT, this, OnShoot);
        //NetworkEvents.Singleton.RegisterEvent(NetworkEvent.PLAYER_RELEASE, this, OnRelease);

        position = fill.rectTransform.sizeDelta;
        position.y = 0.0f;

        height = background.rectTransform.GetHeight();
        targetHeight = height * .8f;

        Reset();
    }

    private void Update()
    {
        if (m_isShooting)
        {
            RectTransformExtensions.SetHeight(m_rectTrans, RectTransformExtensions.GetHeight(m_rectTrans) + (speed * Time.deltaTime));

            // Overflowed bar => Failed shot + disable
            if (RectTransformExtensions.GetHeight(m_rectTrans) >= height)
            {
                StopShooting();
                StartCoroutine(Hide(1.0f));
                print("failed");
            }
        }

    }

    public void OnShoot(Player p, float speedMod, float startOffset, float endOffset)
    {
        speed = BASE_SPEED * speedMod;

        RectTransformExtensions.SetHeight(m_rectTrans, 0.0f + startOffset);

        target.rectTransform.localPosition = GetBarPosition(endOffset);
        
        meter.SetActive(true);
        m_isShooting = true;
        //StartCoroutine(ShootingTimeout());
    }

    public void OnRelease(Player player)
    {
        float dist = Mathf.Abs(RectTransformExtensions.GetHeight(m_rectTrans) - targetHeight);

        if (dist < .5)
        {
            glow.gameObject.SetActive(true);
        }
        print("dist " + dist);
        StopShooting();
        StartCoroutine(Hide(3.0f));
    }

    public Vector3 GetBarPosition(float t_height)
    {
        Vector3 pos = targetPos;
        pos.y = targetHeight + t_height;
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
        meter.SetActive(false);
        glow.gameObject.SetActive(false);
        target.rectTransform.localPosition = Vector3.zero;
        height = background.rectTransform.rect.height;
        fill.rectTransform.sizeDelta = position;
    }
}
