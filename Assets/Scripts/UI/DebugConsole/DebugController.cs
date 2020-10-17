using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    public static DebugController Singleton { get; private set; }

    private const int BUFFER_SIZE = 128;
    private const int VIEW_LENGTH = 16;

    private const float LINE_SIZE = 16;
    private const float VIEW_HEIGHT = VIEW_LENGTH * LINE_SIZE;
    private const float VIEW_BORDER_SIZE = LINE_SIZE / 2;

    private const int LAST_LIST_SIZE = 8;
    private const int TABLE_WIDTH_SIZE = 48;



    private Controls m_controls;
    private bool m_showConsole;

    private string m_input;
    private string m_current;
    private int m_index = -1;
    private Vector2 m_scroll;
    private Vector2 m_hintScroll;
    private Queue<ConsoleText> m_buffer = new Queue<ConsoleText>(BUFFER_SIZE);
    private List<string> m_hints;
    private List<string> m_last = new List<string>();

    private GUIStyle m_textStyle = new GUIStyle();
    private GUIStyle m_textIStyle = new GUIStyle();
    private GUIStyle m_textWStyle = new GUIStyle();
    private GUIStyle m_textEStyle = new GUIStyle();
    private GUIStyle m_hintStyle = new GUIStyle();
    private GUIStyle m_hintSelectStyle = new GUIStyle();

    private List<DebugCommandBase> m_commandList = new List<DebugCommandBase>();

    [SerializeField]
    private Font m_font;

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
            if (string.IsNullOrEmpty(m_input)) return;
            HandleInput();

            m_last.Add(m_input);

            m_index = 0;
            m_input = "";
            m_current = "";
        };

        m_controls.UI.Arrows.performed += ctx => {
            if (m_hints == null) return;
            var val = ctx.ReadValue<Vector2>();

            if (val.y < .5)
            {
                if (m_index >= m_hints.Count) return;
                m_index++;
            }
            else if (val.y > -.5f)
            {
                if (m_index <= -m_last.Count) return;
                m_index--;
            }

            int i = m_last.Count - Mathf.Abs(m_index);
            if (m_index < 0 && !string.IsNullOrEmpty(m_last[i]))
                m_input = m_last[i];
            else if (m_index == 0)
                m_input = m_current;
        };

        m_controls.UI.Tab.performed += ctx => {
            if (m_hints == null || m_hints.Count < 1) return;
            m_input = m_hints[m_index].Split(new char[] { ' ' })[0];
        };

        Keyboard.current.onTextInput += text => {
            if (m_index == 0)
                m_current = m_input;
        };


        m_textStyle.fontSize = 14;
        m_textStyle.font = m_font;
        m_textStyle.normal.textColor = Color.white;

        m_textIStyle.fontSize = 14;
        m_textIStyle.font = m_font;
        m_textIStyle.normal.textColor = new Color(.55f, .8f, 1);

        m_textWStyle.fontSize = 14;
        m_textWStyle.font = m_font;
        m_textWStyle.normal.textColor = new Color(1, .55f, .2f);

        m_textEStyle.fontSize = 14;
        m_textEStyle.font = m_font;
        m_textEStyle.normal.textColor = new Color(1, .3f, .3f);

        m_hintStyle.fontSize = 14;
        m_hintStyle.font = m_font;
        m_hintStyle.normal.textColor = new Color(.6f, .6f, .6f);

        m_hintSelectStyle.fontSize = 14;
        m_hintSelectStyle.font = m_font;
        m_hintSelectStyle.normal.textColor = new Color(200 / 255, 200 / 255, 200 / 255);

        PrintConsole("Testing This 1!", LogType.INFO);
        PrintConsole("Test That 2.", LogType.WARNING);
        PrintConsole("Test These 3?", LogType.ERROR);
        PrintConsoleTable("", new string[] { "test", "this", "table", "LONG_WORD_STRING" },
            new object[] { 1, true, 50.50, "this"}, LogType.WARNING);

        var TEST_CMD = new DebugCommand("test", "testing", "test - testing", args => Debug.Log("testing command"));
        m_commandList.Add(TEST_CMD);

        var TESTDEBUG_CMD = new DebugArgsCommand("test_this", "testing", "test_this <value>", args => Debug.Log("test " + args[0]), 1);
        m_commandList.Add(TESTDEBUG_CMD);
    }

    private void OnGUI()
    {
        if (!m_showConsole) return;

        float y = 0;

        GUI.Box(new Rect(0, y, Screen.width, VIEW_HEIGHT), "");

        Rect viewport = new Rect(0, 0, Screen.width - 30, LINE_SIZE * BUFFER_SIZE);

        m_scroll = GUI.BeginScrollView(new Rect(0, y + VIEW_BORDER_SIZE, Screen.width, LINE_SIZE * VIEW_LENGTH - VIEW_BORDER_SIZE), m_scroll, viewport);
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

            m_hints = MatchStringToCommand(m_input);
            for (int j = 0; j < m_hints.Count; j++)
            {
                if (m_hints[j] == null || string.IsNullOrEmpty(m_hints[j])) continue;
                GUI.Box(new Rect(0, (LINE_SIZE + 10) * j, Screen.width - 30, LINE_SIZE + 10), "");
                Rect labelRect = new Rect(VIEW_BORDER_SIZE, (LINE_SIZE + 10) * j + 5, viewport.width - 100, LINE_SIZE);
                if (j == m_index - 1)
                    GUI.Label(labelRect, m_hints[j], m_hintSelectStyle);
                else
                    GUI.Label(labelRect, m_hints[j], m_hintStyle);
            }
            GUI.EndScrollView();
        }
        GUI.backgroundColor = new Color(0, 0, 0, 0);
    }

    private void HandleInput()
    {
        PrintConsole(m_input, LogType.NONE);
        for (int i = 0; i < m_commandList.Count; i++)
        {
            DebugCommandBase cmd = m_commandList[i];
            string[] split = m_input.Split(new char[] { ' ' });
            string cmdName = split[0];

            List<string> args = new List<string>();
            if (split.Length > 1) // Has a cmdName and args
            {
                for (int j = 1; j < split.Length; j++)
                {
                    if (string.IsNullOrEmpty(split[j])) continue;
                    args.Add(split[j]);
                }
            }

            if (cmdName.Equals(cmd.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (cmd.GetType() == typeof(DebugCommand))
                {
                    ((DebugCommand)cmd).Invoke(cmdName, split);
                }
                else if (cmd.GetType() == typeof(DebugArgsCommand))
                {
                    ((DebugArgsCommand)cmd).Invoke(cmdName, args) ;
                }
            }
        }
    }

    private List<string> MatchStringToCommand(string str)
    {
        List<string> res = new List<string>();
        string pattern = string.Format("^(?i:{0})", Regex.Escape(str));
        
        for (int i = 0; i < m_commandList.Count; i++)
        {
            var m = Regex.Match(m_commandList[i].Name, pattern);
            if (m.Success)
            {
                res.Add(m_commandList[i].Formatted);
            }
        }
        return res;
    }

    public void PrintConsole(string text, LogType type = LogType.INFO)
    {
        ConsoleText cText = new ConsoleText(FormatLog(text, type, false), type);
        AddText(cText);
    }

    public void PrintConsoleServer(string text, LogType type = LogType.INFO)
    {
        ConsoleText cText = new ConsoleText(FormatLog(text, type, true), type);
        MLAPI.Logging.NetworkLog.LogInfoServer(cText.text);
        AddText(cText);
    }

    public void PrintConsoleValues(string text, object[] values, LogType type = LogType.INFO)
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
        AddText(cText);
    }

    public void PrintConsoleTable(string text, string[] keys, object[] values, LogType type = LogType.INFO)
    {
        const int MAX_LENGTH = 32;

        ConsoleText cText = new ConsoleText(GetRepeatedStr('-', MAX_LENGTH), type);
        AddText(cText);

        for (int i = 0; i < values.Length; i++)
        {
            string key;
            object val;

            if (i >= keys.Length || string.IsNullOrEmpty(keys[i]))
                key = "";
            else
                key = keys[i];

            if (values[i] == null)
                val = "NULL";
            else
                val = values[i];

            ConsoleText ct = new ConsoleText(FormatLogTable(key, val.ToString(), "*", MAX_LENGTH), type);
            AddText(ct);
        }
        AddText(cText);
    }

    public void PrintObjAsTable(object obj, LogType type = LogType.INFO)
    {
        const int MAX_LENGTH = 48;

        ConsoleText cText = new ConsoleText(GetRepeatedStr('-', MAX_LENGTH), type);
        AddText(cText);

        ConsoleText headText = new ConsoleText(FormatHeaderTable(obj.GetType().Name, "", MAX_LENGTH), type);
        AddText(headText);

        AddText(cText);

        var fields = obj.GetType().GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            string val = "";
            if (fields[i].GetValue(obj) != null) val = fields[i].GetValue(obj).ToString();

            ConsoleText ct = new ConsoleText(FormatLogTable(fields[i].Name, val, "*", MAX_LENGTH), type);
            AddText(ct);
        }
        AddText(cText);
    }

    private void AddText(ConsoleText cText)
    {
        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(cText);
    }

    private string FormatHeaderTable(string text, string border, int length)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("| ");
        sb.Append(text);
        for (int i = sb.Length; i < length - 1; i++)
        {
            sb.Append(" ");
        }
        sb.Append('|');
        return sb.ToString();
    }

    private string FormatLogTable(string key, string val, string border, int length)
    {
        int halfLength = length / 2;
        StringBuilder sb = new StringBuilder();

        sb.Append('|');
        sb.Append(" ");
        for (int i = 0; i < halfLength - 4; i++)
        {
            if (i < key.Length)
                sb.Append(key[i]);
            else
                sb.Append(" ");
        }
        sb.Append(" ");
        sb.Append('|');
        sb.Append(" ");
        for (int i = 0; i < halfLength - 4; i++)
        {
            if (i < val.Length)
                sb.Append(val[i]);
            else
                sb.Append(" ");
        }
        sb.Append(" ");
        sb.Append(" ");
        sb.Append('|');

        return sb.ToString();
    }

    private string GetRepeatedStr(char chr, int length)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chr);
        }
        return sb.ToString();
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

interface IPrintable
{
    void Print();
}
