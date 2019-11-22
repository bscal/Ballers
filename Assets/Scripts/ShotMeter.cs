using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{

    float speed = 300.0f;
    
    public float height;
    public Vector2 position;
    public Vector3 targetPos = Vector3.zero;

    public GameObject meter;
    public RawImage background;
    public RawImage fill;
    public Image target;
    public RawImage glow;
    
    private bool m_isActive = false;

    private IEnumerator m_coroutine;
    // Start is called before the first frame update
    void Start()
    {
        position = fill.rectTransform.sizeDelta;
        position.y = 0.0f;
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        Debugger.Instance.Print(string.Format("{0},{1},{2}", target.rectTransform.localPosition.y, fill.rectTransform.rect.height, target.rectTransform.localPosition.y - fill.rectTransform.rect.height), 3);
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (m_isActive)
            {
                m_isActive = false;
                float dist = Mathf.Abs(target.rectTransform.localPosition.y - fill.rectTransform.rect.height);
                print(dist);

                if (dist < .5)
                {
                    glow.gameObject.SetActive(true);
                }

                StartCoroutine(Hide(3.0f));
            }
            else
            {
                target.rectTransform.localPosition = GetBarPosition(1);
                fill.rectTransform.sizeDelta = position;
                meter.SetActive(true);
                m_isActive = true;
            }
        }

        if (m_isActive)
        {
            fill.rectTransform.sizeDelta = fill.rectTransform.sizeDelta + Vector2.up * (speed * Time.deltaTime);
        }

        if (m_isActive && fill.rectTransform.rect.height >= height)
        {
            m_isActive = false;
            StartCoroutine(Hide(1.0f));
            print("failed");
        }
    }

    /// <summary>
    /// When a player takes a shot that requires the shotmeter.
    /// 
    /// </summary>
    /// <param name="t_p">The player</param>
    public void OnShot(Player t_p)
    {
        // When a player takes a shot

        // Random speed ?
        // What speed ?
        // Set speed

        // Random target location?
        
        // Random start location?

        // Skill 
    }

    public Vector3 GetBarPosition(int t_rating)
    {
        Vector3 pos = targetPos;
        pos.y = target.rectTransform.localPosition.y + (height * .8f);
        return pos;
    }

    IEnumerator Hide(float t_wait)
    {
        yield return new WaitForSecondsRealtime(t_wait);
        Reset();
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
