using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyType
{
    PRESS,
    DOUBLE_PRESS,
    HOLD
}

public class KeyData
{
    public float timer;
    public bool pressed;
    public bool doubled;
    public bool held;
}

public class DribbleControls : MonoBehaviour
{

    private const float COUNT_HELD_DOWN         = 0.3f;
    private const float COUNT_DOUBLE_PRESS      = 0.3f;

    // WASD for ball handling Arrow keys for controls
    // TODO

    private Dictionary<KeyCode, KeyData> m_keyData = new Dictionary<KeyCode, KeyData>();
    private KeyCode[] m_combo = new KeyCode[3];

    private bool keyPressed = false;

    private void Awake()
    {
        m_keyData.Add(KeyCode.W, new KeyData());
        m_keyData.Add(KeyCode.A, new KeyData());
        m_keyData.Add(KeyCode.S, new KeyData());
        m_keyData.Add(KeyCode.D, new KeyData());
    }

    private void Start()
    {
        StartCoroutine(DribbleCombo());
    }

    private IEnumerator DribbleCombo()
    {
        while (true)
        {
            foreach (var pair in m_keyData)
            {
                if (Input.GetKeyDown(pair.Key))
                {
                    if (Input.GetKey(pair.Key))
                    {
                        StartCoroutine(Held(pair.Key));
                    }
                }

                if (Input.GetKeyUp(pair.Key))
                {
                    pair.Value.held = false;
                    //Check if we have not not reached the timer then it is only a key press
                    if (pair.Value.timer < COUNT_HELD_DOWN)
                    {
                        if (pair.Value.pressed)
                        {
                            pair.Value.doubled = true;
                            yield return OnKeyDoublePressed(pair.Key);
                        }
                        else
                        {
                            StartCoroutine(DoublePress(pair.Key));
                        }
                    }
                    //Reset timer to 0 for the next key press
                    pair.Value.timer = 0f;
                }
            }
            yield return null;
        }
    }

    private IEnumerator DoublePress(KeyCode key)
    {
        if (m_keyData.TryGetValue(key, out KeyData value))
        {
            value.pressed = true;
            yield return new WaitForSeconds(COUNT_DOUBLE_PRESS);
            value.pressed = false;
            if (!value.doubled)
                yield return OnKeyPressed(key);
            value.doubled = false;
        }
    }

    private IEnumerator Held(KeyCode key)
    {
        if (m_keyData.TryGetValue(key, out KeyData value))
        {
            value.held = true;
            while (value.held)
            {
                //Start incrementing timer
                value.timer += Time.deltaTime;

                //Check if this counts as being "Held Down"
                if (value.timer > COUNT_HELD_DOWN)
                {
                    //It a "key held down", call the OnKeyHeldDown function and wait for it to return
                    yield return OnKeyHeldDown(key);
                    break;
                }

                yield return null;
            }
            value.held = false;
        }
    }

    private IEnumerator OnKeyPressed(KeyCode key)
    {
        print("pressed Key: " + key);
        yield return null;
    }

    private IEnumerator OnKeyDoublePressed(KeyCode key)
    {
        print("double pressed Key: " + key);
        yield return null;
    }

    private IEnumerator OnKeyHeldDown(KeyCode key)
    {
        print("Held Key: " + key);
        yield return null;
    }

}
