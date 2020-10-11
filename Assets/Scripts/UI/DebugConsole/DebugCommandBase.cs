using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DebugCommandError
{
    ERROR,
    ARG
}

public abstract class DebugCommandBase
{
    private readonly string m_name;
    private readonly string m_desc;
    private readonly string m_formatted;

    public string Name { get { return m_name; } }
    public string Description { get { return m_desc; } }
    public string Formatted { get { return m_formatted; } }

    protected DebugCommandBase(string name, string desc, string format)
    {
        m_name = name;
        m_desc = desc;
        m_formatted = format;
    }

    public virtual void HandleError(string err)
    {
        DebugController.Singleton.PrintConsole(err, LogType.ERROR);
    }
}
