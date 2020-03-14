using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DribbleMove
{
    CROSSOVER,
    LONG_CROSSOVER,
    SPIN,
    BEHIND_BACK,
    BETWEEN_LEG,
    JAB,
    HESTITATION,
    STEP_BACK
}

public class Combo
{
    public string name;

    public Combo(string name)
    {
        this.name = name;
    }
}

public class KeyData
{
    public float timer;
    public bool pressed;
    public bool doubled;
    public bool held;
    public bool shift;
    public bool ctrl;
    public bool alt;
}

public class KeyCombo
{
    public KeyCode key;
    public bool pressed;
    public bool doubled;
    public bool held;
    public bool shift;
    public bool ctrl;
    public bool alt;

    public KeyCombo() { }
    public KeyCombo(KeyCode key, bool pressed, bool doubled, bool held, bool shift, bool ctrl, bool alt)
    {
        this.key = key;
        this.pressed = pressed;
        this.doubled = doubled;
        this.held = held;
        this.shift = shift;
        this.ctrl = ctrl;
        this.alt = alt;
    }

    public override int GetHashCode()
    {
        unchecked // only needed if you're compiling with arithmetic checks enabled
        { // (the default compiler behaviour is *disabled*, so most folks won't need this)
            int hash = 13;
            hash = (hash * 7) + key.GetHashCode();
            hash = (hash * 7) + pressed.GetHashCode();
            hash = (hash * 7) + doubled.GetHashCode();
            hash = (hash * 7) + held.GetHashCode();
            hash = (hash * 7) + shift.GetHashCode();
            hash = (hash * 7) + ctrl.GetHashCode();
            hash = (hash * 7) + alt.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (GetType() != obj.GetType()) return false;
        return GetHashCode().Equals(obj.GetHashCode());
    }
}

public class ComboList<KeyCombo> : List<KeyCombo>
{
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 0;
            ForEach((combo) => { hash += combo.GetHashCode(); });
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (GetType() != obj.GetType()) return false;
        return GetHashCode().Equals(obj.GetHashCode());
    }
}

public class DribbleControls : MonoBehaviour
{
    // Constants
    private const float COUNT_HELD_DOWN         = 0.3f;
    private const float COUNT_DOUBLE_PRESS      = 0.3f;
    private const float COMBO_TIMEOUT           = 0.6f;
    private const int MAX_COMBO                 = 3;

    // Key Data
    private Dictionary<KeyCode, KeyData> m_keyData = new Dictionary<KeyCode, KeyData>();

    // Combo Map
    private Dictionary<ComboList<KeyCombo>, Combo> m_combos = new Dictionary<ComboList<KeyCombo>, Combo>();

    // Combo Variables
    private List<KeyCode> m_combo = new List<KeyCode>();
    private ComboList<KeyCombo> m_comboMove = new ComboList<KeyCombo>();
    private bool m_inputHappened = false;
    private float m_comboTimer = 0.0f;
    private float m_endTime = 0.0f;

    private void Awake()
    {
        m_keyData.Add(KeyCode.W, new KeyData());
        m_keyData.Add(KeyCode.A, new KeyData());
        m_keyData.Add(KeyCode.S, new KeyData());
        m_keyData.Add(KeyCode.D, new KeyData());

        KeyCombo crossL = new KeyCombo(KeyCode.A, true, false, false, false, false, false);
        KeyCombo crossR = new KeyCombo(KeyCode.D, true, false, false, false, false, false);

        KeyCombo jab = new KeyCombo();
        jab.key = KeyCode.W;
        jab.doubled = true;

        KeyCombo test = new KeyCombo();
        test.key = KeyCode.S;
        test.held = true;
        test.shift = true;

        m_combos.Add(new ComboList<KeyCombo> { crossL, crossR }, new Combo("Double Cross"));

        m_combos.Add(new ComboList<KeyCombo> { jab, jab }, new Combo("Double Jab"));

        m_combos.Add(new ComboList<KeyCombo> { test }, new Combo("test test"));

    }

    private void Update()
    {
        // Checks each resisted key if it is held.
        foreach (var pair in m_keyData)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                if (Input.GetKey(pair.Key))
                {
                    // Start a Coroutine to track if key is held
                    StartCoroutine(Held(pair.Key));
                }
            }

            if (Input.GetKeyUp(pair.Key))
            {
                // Stops the held coroutine by setting held to false
                pair.Value.held = false;
                //Check if we have not not reached the timer then it is only a key press
                if (pair.Value.timer < COUNT_HELD_DOWN)
                {
                    // Will stop the DoublePress coroutine if key is already pressed
                    if (pair.Value.pressed)
                    {
                        pair.Value.doubled = true;
                    }
                    else
                    {
                        // Starts coroutine to check if key is pressed twice
                        StartCoroutine(DoublePress(pair.Key));
                    }
                }
                // Reset timer to 0 for the next key press
                pair.Value.timer = 0f;
            }
        }
        // Updates combos
        UpdateCombo();
    }

    private void UpdateCombo()
    {
        if (!m_inputHappened) return;
        m_comboTimer += Time.deltaTime;
        // combo timer timeout
        if (m_comboTimer > m_endTime)
        {
            HandleCombo();
            Cleanup();
        }
    }

    private IEnumerator DoublePress(KeyCode key)
    {
        if (m_keyData.TryGetValue(key, out KeyData value))
        {
            value.pressed = true;
            yield return new WaitForSeconds(COUNT_DOUBLE_PRESS);
            value.pressed = false;
            if (value.doubled)
                yield return OnKeyDoublePressed(key);
            else
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
        OnInput(key, true, false, false);
        yield return null;
    }

    private IEnumerator OnKeyDoublePressed(KeyCode key)
    {
        OnInput(key, false, true, false);
        yield return null;
    }

    private IEnumerator OnKeyHeldDown(KeyCode key)
    {
        OnInput(key, false, false, true);
        yield return null;
    }

    private void OnInput(KeyCode key, bool pressed, bool doubled, bool held)
    {
        m_inputHappened = true;
        m_combo.Add(key);
        m_comboMove.Add(new KeyCombo(key, pressed, doubled, held, Input.GetKey(KeyCode.LeftShift), Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.LeftAlt)));

        if (m_combo.Count > MAX_COMBO)
            HandleCombo();
        else
            m_endTime = m_comboTimer + COMBO_TIMEOUT;
    }

    private void HandleCombo()
    {
        foreach (var c in m_comboMove)
        {
            print(c.GetHashCode());
        }
        if (m_combos.TryGetValue(m_comboMove, out Combo combo))
        {
            print(combo.name);
        }

        Cleanup();
    }

    private void Cleanup()
    {
        m_combo.Clear();
        m_comboMove.Clear();
        m_inputHappened = false;
        m_comboTimer = 0f;
        m_endTime = COMBO_TIMEOUT;
    }

}

