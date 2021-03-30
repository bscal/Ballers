using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private readonly Dictionary<InputAction, uint> m_map = new Dictionary<InputAction, uint>();
    private Controls m_controls;

    private void Start()
    {
        m_controls = new Controls();
        m_controls.Enable();

        foreach (var action in m_controls.Keyboard.Get().actions)
        {
            action.performed += ctx => {
                if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
                {
                    if (!m_map.ContainsKey(ctx.action)) return;
                    //NetInput.SendInput(m_map[ctx.action], InputType.PRESSED);
                }
            };

            action.canceled += ctx => {
                if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
                {
                    if (!m_map.ContainsKey(ctx.action)) return;
                    //NetInput.SendInput(m_map[ctx.action], InputType.RELEASED);
                }
            };
        }

        RebindMap();
    }

    public void RebindMap()
    {
        if (m_map.Count > 0)
        {
            m_map.Clear();
        }

        InputID.Reset();

        foreach (var action in m_controls.Keyboard.Get().actions)
        {
            m_map.Add(action, InputID.GetNext());
        }
    }
}

public enum InputType
{
    NONE        = 0,
    PRESSED     = 1,
    RELEASED    = 2
}

static class InputID
{
    private static uint m_increment = 0;

    public static uint GetNext()
    {
        return m_increment++;
    }

    public static void Reset()
    {
        m_increment = 0;
    }
}

