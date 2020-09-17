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
    private readonly DribbleControls m_controls;
    private readonly InputAction m_action;

    public float ReadValue { get { return m_action.ReadValue<float>(); } }
    public string Bind { get { return m_action.name; } }
    public bool Pressed { get; set; }
    public bool Released { get; set; }
    public bool Held { get; set; }

    public ControlKey(DribbleControls controls, string bind)
    {
        m_controls = controls;
        m_action = new InputAction("dribble" + bind, type: InputActionType.Value, binding: "*/" + bind);
        m_action.Enable();

        m_action.started += ctx => {
            Pressed = true;
            m_controls.StartCoroutine(m_controls.ResetPress(this));
            m_controls.StartCoroutine(m_controls.IsHeld(this));
        };


        m_action.canceled += ctx => {
            Released = true;
            Held = false;
            m_controls.StartCoroutine(m_controls.ResetRelease(this));
        };
    }
}

public class DribbleControls : MonoBehaviour
{
    // Constants
    private const float COUNT_HELD_DOWN     = 0.6f;
    private const float COUNT_DOUBLE_PRESS  = 0.3f;
    private const float COMBO_TIMEOUT       = 1.5f;
    private const int MAX_COMBO             = 3;
    
    // Key Data
    private readonly Dictionary<ControlKey, KeyData> m_keyData = new Dictionary<ControlKey, KeyData>();

    // Combo Map
    private readonly Dictionary<ComboList<KeyCombo>, Combo> m_combos = new Dictionary<ComboList<KeyCombo>, Combo>();

    // Combo Variables
    private readonly List<ControlKey> m_combo = new List<ControlKey>();
    private readonly ComboList<KeyCombo> m_comboMove = new ComboList<KeyCombo>();
    private bool m_inputHappened;
    private float m_comboTimer;
    private float m_endTime;

    private Controls actions;
    private ControlKey actionW;
    private ControlKey actionS;
    private ControlKey actionA;
    private ControlKey actionD;

    private void OnEnable()
    {
        actions = new Controls();
        actions.Enable();
        actionW = new ControlKey(this, "W");
        actionS = new ControlKey(this, "S");
        actionA = new ControlKey(this, "A");
        actionD = new ControlKey(this, "D");
        m_keyData[actionW] = new KeyData();
        m_keyData[actionS] = new KeyData();
        m_keyData[actionA] = new KeyData();
        m_keyData[actionD] = new KeyData();

        KeyCombo crossL = new KeyCombo(actionA, true, false, false, false, false, false);
        KeyCombo crossR = new KeyCombo(actionD, true, false, false, false, false, false);

        KeyCombo jab = new KeyCombo {
            key = actionW,
            doubled = true
        };

        KeyCombo test = new KeyCombo {
            key = actionS,
            held = true,
            shift = true
        };

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
            if (pair.Key.Pressed && !pair.Key.Held && !pair.Value.triggered)
            {
                StartCoroutine(KeyPress(pair.Key, pair.Value));
            }
        }

        // Updates combos
        UpdateCombo();
    }

    private IEnumerator KeyPress(ControlKey key, KeyData data)
    {
        data.Reset();
        data.triggered = true;
        float timer = 0;
        bool run = true;
        while (run)
        {
            timer += Time.deltaTime;

            if (timer > COUNT_HELD_DOWN) // held
            {
                yield return OnKeyHeldDown(key);
                run = false;
            }

            if (key.Released)
            {
                while (true)
                {
                    yield return null;
                    data.timer += Time.deltaTime;
                    if (key.Pressed) // double tapped
                    {
                        yield return OnKeyDoublePressed(key);
                        break;
                    }
                    if (data.timer > COUNT_DOUBLE_PRESS) // single tapped
                    {
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
        print("Key " + key + ", " + pressed + ", " + doubled + ", " + held);
        m_inputHappened = true;
        m_combo.Add(key);
        m_comboMove.Add(new KeyCombo(key, pressed, doubled, held,
            Keyboard.current.leftShiftKey.isPressed,
            Keyboard.current.leftCtrlKey.isPressed,
            Keyboard.current.leftAltKey.isPressed));

        if (m_combo.Count > MAX_COMBO)
            HandleCombo();
        else
        {
            m_endTime = COMBO_TIMEOUT;
            m_comboTimer = 0f;
        }
            
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
        m_endTime = 0f;
    }

    // ============== Coroutines for ControlKeys ==============

    public IEnumerator ResetPress(ControlKey obj)
    {
        yield return null;
        obj.Pressed = false;
    }

    public IEnumerator IsHeld(ControlKey obj)
    {
        float timer = 0;
        while (timer < .3f)
        {
            yield return null;
            timer += Time.deltaTime;
            if (obj.Released)
                break;
        }
        if (!obj.Released)
            obj.Held = true;

    }

    public IEnumerator ResetRelease(ControlKey obj)
    {
        yield return null;
        obj.Released = false;
    }

}