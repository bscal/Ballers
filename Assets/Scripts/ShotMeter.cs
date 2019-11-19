using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour
{

    public float height;
    public Vector2 position;

    public GameObject meter;
    public RawImage background;
    public RawImage fill;
    public Image target;
    public GameObject glowObject;
    public RawImage glow;

    private bool m_isActive = false;

    private readonly float m_max = 120.0f;
    private readonly float m_min = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        glowObject.SetActive(false);
        position = fill.rectTransform.sizeDelta;
        position.y = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            meter.SetActive(true);
            fill.rectTransform.sizeDelta = position;
            m_isActive = true;
        }

        if (m_isActive)
        {
            fill.rectTransform.sizeDelta = fill.rectTransform.sizeDelta + Vector2.up;
        }

        if (m_isActive && fill.rectTransform.rect.height >= height)
        {
            glowObject.SetActive(true);
            m_isActive = false;
            print("failed");
        }
    }

    public float GetBarPosition(int rating)
    {
        return rating;
    }
}
