using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyInput
{
    private const float COUNT_HELD_DOWN = 0.4f;

    private readonly InputAction m_action;

    public float ReadValue { get { return m_action.ReadValue<float>(); } }
    public string Bind { get { return m_action.name; } }
    public bool Pressed { get; set; }
    public bool Released { get; set; }
    public bool DoublePressed { get; set; }
    public bool Held { get; set; }

    private float m_startTimer;
    private bool m_pressedOnce;
    private bool m_shouldResetPress;
    private bool m_shouldResetRelease;

    public KeyInput(string bind)
    {
        m_action = new InputAction("keyInput" + bind, type: InputActionType.Value, binding: "*/" + bind);
        m_action.Enable();

        m_action.started += ctx => {
            Pressed = true;
            m_shouldResetPress = true;

            if (m_pressedOnce)
                DoublePressed = true;

            m_startTimer = Time.time;
        };

        m_action.canceled += ctx => {
            Released = true;
            Held = false;
            m_shouldResetRelease = true;
        };
    }

    public void Update()
    {
        if (m_startTimer + COUNT_HELD_DOWN < Time.time)
            Held = true;
        else if (m_pressedOnce)
            m_pressedOnce = false;

        if (Released)
        {
            m_pressedOnce = true;
            m_startTimer = Time.time;
        }
    }

    public void LateUpdate()
    {
        if (m_shouldResetPress)
        {
            Pressed = false;
            DoublePressed = false;
        }
        if (m_shouldResetRelease)
        {
            Released = false;
        }
    }
}

/// <summary>
/// Class that handles more complex key binds built from InputSystem
/// </summary>
public class ControlsManager : MonoBehaviour
{
    private readonly Dictionary<string, KeyInput> m_bindMap = new Dictionary<string, KeyInput>();

    private void Update()
    {
        foreach (var pair in m_bindMap)
        {
            pair.Value.Update();
        }
    }

    private void LateUpdate()
    {
        foreach (var pair in m_bindMap)
        {
            pair.Value.LateUpdate();
        }
    }

    public void AddNewKeyInput(KeyInput keyInput)
    {
        m_bindMap.Add(keyInput.Bind, keyInput);
    }

}
