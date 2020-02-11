using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{

    float speed = 400.0f;
    
    public float height;
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

        SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Shoot += OnShoot;
        SpawnManager.GetLocalPlayerObject().GetComponent<Player>().Release += OnRelease;

        position = fill.rectTransform.sizeDelta;
        position.y = 0.0f;

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

    /// <summary>
    /// When a player takes a shot that requires the shotmeter.
    /// 
    /// </summary>
    /// <param name="t_p">The player</param>
    public void OnShoot(Player t_p)
    {

        // When a player takes a shot

        // Random speed ?
        // What speed ?
        // Set speed

        // Random target location?

        // Random start location?

        // Skill

        target.rectTransform.localPosition = GetBarPosition(1);
        RectTransformExtensions.SetHeight(m_rectTrans, 0.0f);
        meter.SetActive(true);
        m_isShooting = true;
        StartCoroutine(ShootingTimeout());
    }

    public void OnRelease(Player player)
    {
        float dist = Mathf.Abs(target.rectTransform.localPosition.y - fill.rectTransform.rect.height);

        if (dist < .5)
        {
            glow.gameObject.SetActive(true);
        }

        StopShooting();
        StartCoroutine(Hide(3.0f));
    }

    public Vector3 GetBarPosition(int t_rating)
    {
        Vector3 pos = targetPos;
        pos.y = target.rectTransform.localPosition.y + (height * .8f);
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
