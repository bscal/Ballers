using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
    public bool triggered;
    public bool pressed;
    public bool doubled;
    public bool held;
    public bool shift;
    public bool ctrl;
    public bool alt;

    public void Reset()
    {
        timer = 0;
        triggered = false;
        pressed = false;
        doubled = false;
        held = false;
        shift = false;
        ctrl = false;
        alt = false;
    }
}

public class KeyCombo
{
    public ControlKey key;
    public bool pressed;
    public bool doubled;
    public bool held;
    public bool shift;
    public bool ctrl;
    public bool alt;

    public KeyCombo() { }
    public KeyCombo(ControlKey key, bool pressed, bool doubled, bool held, bool shift, bool ctrl, bool alt)
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
public class ControlKey
{
    public readonly InputAction action;
    public readonly string key;

    public bool Pressed { get { return action.ReadValue<float>() > 0; } }

    public bool performed;
    public bool released;

    private bool m_waiting;

    public ControlKey(string key)
    {
        this.key = key;
        action = new InputAction("dribble" + key, type: InputActionType.Value, binding: "*/" + key);
        action.Enable();

        action.performed += ctx => {
            if (!m_waiting) performed = true;
        };

        action.canceled += ctx => {
            performed = false;
            released = true;
            m_waiting = true;
            LeanTween.delayedCall(.1f, () => {
                released = false;
                m_waiting = false;
            });
        };
    }
}

public class DribbleControls : MonoBehaviour
{
    // Constants
    private const float COUNT_HELD_DOWN         = 0.6f;
    private const float COUNT_DOUBLE_PRESS      = 0.3f;
    private const float COMBO_TIMEOUT           = 0.9f;
    private const int MAX_COMBO                 = 3;

    // Key Data
    private Dictionary<ControlKey, KeyData> m_keyData = new Dictionary<ControlKey, KeyData>();

    // Combo Map
    private Dictionary<ComboList<KeyCombo>, Combo> m_combos = new Dictionary<ComboList<KeyCombo>, Combo>();

    // Combo Variables
    private List<ControlKey> m_combo = new List<ControlKey>();
    private ComboList<KeyCombo> m_comboMove = new ComboList<KeyCombo>();
    private bool m_inputHappened = false;
    private float m_comboTimer = 0.0f;
    private float m_endTime = 0.0f;

    private Controls actions;
    private ControlKey actionW;
    private ControlKey actionS;
    private ControlKey actionA;
    private ControlKey actionD;
    private void OnEnable()
    {
        actions = new Controls();
        actions.Enable();
        actionW = new ControlKey("W");
        actionS = new ControlKey("S");
        actionA = new ControlKey("A");
        actionD = new ControlKey("D");
        m_keyData[actionW] = new KeyData();
        m_keyData[actionS] = new KeyData();
        m_keyData[actionA] = new KeyData();
        m_keyData[actionD] = new KeyData();

        KeyCombo crossL = new KeyCombo(actionA, true, false, false, false, false, false);
        KeyCombo crossR = new KeyCombo(actionD, true, false, false, false, false, false);

        KeyCombo jab = new KeyCombo();
        jab.key = actionW;
        jab.doubled = true;

        KeyCombo test = new KeyCombo();
        test.key = actionS;
        test.held = true;
        test.shift = true;

        m_combos.Add(new ComboList<KeyCombo> { crossL, crossR }, new Combo("Double Cross"));

        m_combos.Add(new ComboList<KeyCombo> { jab, jab }, new Combo("Double Jab"));

        m_combos.Add(new ComboList<KeyCombo> { test }, new Combo("test test"));
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    private void Update()
    {
        // Checks each resisted key if it is held.
        foreach (var pair in m_keyData)
        {
            if (pair.Key.performed && !pair.Value.triggered)
            {
                StartCoroutine(KeyPress(pair.Key, pair.Value));
            }
        }

        // Updates combos
        UpdateCombo();
    }

    private IEnumerator KeyPress(ControlKey key, KeyData data)
    {
        print(key.key);
        data.Reset();
        data.triggered = true;
        float timer = 0;
        bool run = true;
        while (run)
        {
            timer += Time.deltaTime;

            if (timer > COUNT_HELD_DOWN)
            {
                // held
                yield return OnKeyHeldDown(key);
                run = false;
            }

            if (key.released)
            {
                while (true)
                {
                    data.timer += Time.deltaTime;
                    if (key.action.ReadValue<float>() > 0)
                    {
                        // double tapped
                        yield return OnKeyDoublePressed(key);
                        break;
                    }
                    yield return null;
                    if (data.timer > COUNT_DOUBLE_PRESS)
                    {
                        // single tapped
                        yield return OnKeyPressed(key);
                        break;
                    }
                }
                run = false;
            }
            yield return null;
        }
        data.triggered = false;
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

    private IEnumerator DoublePress(ControlKey key)
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

    private IEnumerator Held(ControlKey key)
    {
        if (m_keyData.TryGetValue(key, out KeyData value))
        {
            value.held = true;
            value.timer = 0f;
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

    private IEnumerator OnKeyPressed(ControlKey key)
    {
        OnInput(key, true, false, false);
        yield return null;
    }

    private IEnumerator OnKeyDoublePressed(ControlKey key)
    {
        OnInput(key, false, true, false);
        yield return null;
    }

    private IEnumerator OnKeyHeldDown(ControlKey key)
    {
        OnInput(key, false, false, true);
        yield return null;
    }

    private void OnInput(ControlKey key, bool pressed, bool doubled, bool held)
    {
        print("Key " + key.key + ", " + pressed + ", " + doubled + ", " + held);
        m_inputHappened = true;
        m_combo.Add(key);
        m_comboMove.Add(new KeyCombo(key, pressed, doubled, held,
            Keyboard.current.leftShiftKey.isPressed,
            Keyboard.current.leftCtrlKey.isPressed,
            Keyboard.current.leftAltKey.isPressed));

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