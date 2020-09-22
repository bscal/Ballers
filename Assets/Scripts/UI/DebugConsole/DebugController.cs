using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    private const int BUFFER_SIZE = 128;
    private const int VIEW_LENGTH = 16;

    private const float LINE_SIZE = 16;
    private const float VIEW_HEIGHT = VIEW_LENGTH * LINE_SIZE;
    private const float VIEW_BORDER_SIZE = LINE_SIZE / 2;

    private Controls m_controls;
    private bool m_showConsole;

    private string m_input;
    private Vector2 m_scroll;
    private Vector2 m_hintScroll;
    private Queue<string> m_buffer = new Queue<string>(BUFFER_SIZE);

    private GUIStyle m_textStyle = new GUIStyle();
    private GUIStyle m_hintStyle = new GUIStyle();
    private GUIStyle m_boxStyle = new GUIStyle();

    public static DebugCommand TEST_CMD;
    public List<DebugCommandBase> commandList = new List<DebugCommandBase>();

    // Start is called before the first frame update
    void Awake()
    {
        m_controls = new Controls();
        m_controls.Enable();
        m_controls.Keyboard.Console.performed += ctx => {
            m_showConsole = !m_showConsole;
        };

        m_controls.Keyboard.Return.performed += ctx => {
            HandleInput();
            m_input = "";
        };

        m_textStyle.fontSize = 14;
        m_textStyle.normal.textColor = Color.white;

        m_hintStyle.fontSize = 14;
        m_hintStyle.normal.textColor = new Color(.6f, .6f, .6f);

        TEST_CMD = new DebugCommand("test", "testing", "test - testing", args => Debug.Log("test " + args[0]));
        commandList.Add(TEST_CMD);

        var TESTDEBUG_CMD = new DebugCommand("test_this", "testing", "test_this <value>", args => Debug.Log("test " + args[1]));
        commandList.Add(TESTDEBUG_CMD);
    }

    private void OnGUI()
    {
        if (!m_showConsole) return;

        float y = 0;

        GUI.Box(new Rect(0, y, Screen.width, VIEW_HEIGHT), "");

        Rect viewport = new Rect(0, 0, Screen.width - 30, LINE_SIZE * BUFFER_SIZE);

        m_scroll = GUI.BeginScrollView(new Rect(0, y + VIEW_BORDER_SIZE, Screen.width, VIEW_HEIGHT - VIEW_BORDER_SIZE), m_scroll, viewport);
        int i = 0;
        foreach (string line in m_buffer)
        {
            if (string.IsNullOrEmpty(line)) continue;
            Rect labelRect = new Rect(VIEW_BORDER_SIZE, LINE_SIZE * i, viewport.width - 100, LINE_SIZE);
            GUI.Label(labelRect, line, m_textStyle);
            i++;
            if (i >= VIEW_LENGTH) return;
        }

        GUI.EndScrollView();

        y += VIEW_HEIGHT;

        GUI.Box(new Rect(0, y, Screen.width, LINE_SIZE * 2), "");

        m_input = GUI.TextField(new Rect(VIEW_BORDER_SIZE, y + LINE_SIZE / 2, Screen.width - LINE_SIZE, LINE_SIZE), m_input, m_textStyle);
        
        y += LINE_SIZE + LINE_SIZE / 2;

        if (!string.IsNullOrEmpty(m_input))
        {
            m_hintScroll = GUI.BeginScrollView(
                new Rect(0, y + VIEW_BORDER_SIZE, Screen.width, LINE_SIZE * 8 - VIEW_BORDER_SIZE),
                m_hintScroll,
                new Rect(0, 0, Screen.width - 30, LINE_SIZE * 8));

            List<string> hints = MatchStringToCommand(m_input);
            for (int j = 0; j < hints.Count; j++)
            {
                if (hints[j] == null || string.IsNullOrEmpty(hints[j])) continue;
                GUI.Box(new Rect(0, (LINE_SIZE + 10) * j, Screen.width - 30, LINE_SIZE + 10), "");
                Rect labelRect = new Rect(VIEW_BORDER_SIZE, (LINE_SIZE + 10) * j + 5, viewport.width - 100, LINE_SIZE);
                GUI.Label(labelRect, hints[j], m_hintStyle);
            }
            GUI.EndScrollView();
        }
        GUI.backgroundColor = new Color(0, 0, 0, 0);
    }

    private void HandleInput()
    {
        PrintConsole(m_input);
        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase cmd = commandList[i];
            string[] args = m_input.Split(new char[] { ' ' });
            if (args[0].Equals(cmd.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (cmd.GetType() == typeof(DebugCommand))
                {
                    ((DebugCommand)cmd).Invoke(args);
                }
            }
        }
    }

    private List<string> MatchStringToCommand(string str)
    {
        List<string> res = new List<string>();
        string pattern = string.Format("^(?i:{0})", Regex.Escape(str));
        
        for (int i = 0; i < commandList.Count; i++)
        {
            var m = Regex.Match(commandList[i].Name, pattern);
            if (m.Success)
            {
                res.Add(commandList[i].Formatted);
            }
        }
        return res;
    }

    public void PrintConsole(string text)
    {
        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(text);
    }
}
