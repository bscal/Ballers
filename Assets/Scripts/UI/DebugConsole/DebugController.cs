using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    private const int BUFFER_SIZE = 128;
    private const int VIEW_LENGTH = 16;

    private const float LINE_SIZE = 16;
    private const float VIEW_HEIGHT = VIEW_LENGTH * LINE_SIZE;
    private const float VIEW_BORDER_SIZE = LINE_SIZE / 2;

    public static DebugController Singleton { get; private set; }

    private Controls m_controls;
    private bool m_showConsole;

    private string m_input;
    private Vector2 m_scroll;
    private Vector2 m_hintScroll;
    private Queue<ConsoleText> m_buffer = new Queue<ConsoleText>(BUFFER_SIZE);

    private GUIStyle m_textStyle = new GUIStyle();
    private GUIStyle m_textIStyle = new GUIStyle();
    private GUIStyle m_textWStyle = new GUIStyle();
    private GUIStyle m_textEStyle = new GUIStyle();

    private GUIStyle m_hintStyle = new GUIStyle();

    public List<DebugCommandBase> commandList = new List<DebugCommandBase>();

    // Start is called before the first frame update
    void Awake()
    {
        Singleton = this;
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

        m_textIStyle.fontSize = 14;
        m_textIStyle.normal.textColor = new Color(.55f, .8f, 1);

        m_textWStyle.fontSize = 14;
        m_textWStyle.normal.textColor = new Color(1, .55f, .2f);

        m_textEStyle.fontSize = 14;
        m_textEStyle.normal.textColor = new Color(1, .3f, .3f);

        m_hintStyle.fontSize = 14;
        m_hintStyle.normal.textColor = new Color(.6f, .6f, .6f);

        PrintConsole("Testing This 1!", LogType.INFO);
        PrintConsole("Test That 2.", LogType.WARNING);
        PrintConsole("Test These 3?", LogType.ERROR);
        PrintConsoleValues("TestValues", new object[]{ 1, 5.5, false, null }, LogType.INFO);

        var TEST_CMD = new DebugCommand("test", "testing", "test - testing", args => Debug.Log("test " + args[0]));
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
        foreach (ConsoleText line in m_buffer)
        {
            if (string.IsNullOrEmpty(line.text)) continue;
            Rect labelRect = new Rect(VIEW_BORDER_SIZE, LINE_SIZE * i, viewport.width - 100, LINE_SIZE);
            GUIStyle style;
            if (line.type == LogType.INFO) style = m_textIStyle;
            else if (line.type == LogType.WARNING) style = m_textWStyle;
            else if (line.type == LogType.ERROR) style = m_textEStyle;
            else style = m_textStyle;
            GUI.Label(labelRect, line.text, style);
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
        PrintConsole(m_input, LogType.NONE);
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

    public void PrintConsole(string text, LogType type)
    {
        ConsoleText cText = new ConsoleText(FormatLog(text, type, false), type);

        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(cText);
    }

    public void PrintConsoleServer(string text, LogType type)
    {
        ConsoleText cText = new ConsoleText(FormatLog(text, type, true), type);
        MLAPI.Logging.NetworkLog.LogInfoServer(cText.text);

        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(cText);
    }

    public void PrintConsoleValues(string text, object[] values, LogType type)
    {
        StringBuilder sb = new StringBuilder(text);
        sb.Append(": ");
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == null)
                sb.Append("NULL");
            else
                sb.Append(values[i].GetType().Name + " : " + values[i]);
            if (i != values.Length - 1) sb.Append(" | ");
        }
        ConsoleText cText = new ConsoleText(FormatLog(sb.ToString(), type, false), type);

        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(cText);
    }

    private string FormatLog(string text, LogType type, bool isServer)
    {
        return string.Format("[{0}]{1}{2}: {3}",
            DateTime.Now.ToString("HH:mm:ss"),
            (isServer) ? "[Server] " : "",
            (type == LogType.NONE) ? "" : "[" + type.ToString() + "]",
            text);
    }
}

public enum LogType
{
    NONE,
    INFO,
    WARNING,
    ERROR
}

struct ConsoleText
{
    public string text;
    public LogType type;

    public ConsoleText(string text, LogType type)
    {
        this.text = text;
        this.type = type;
    }
}
