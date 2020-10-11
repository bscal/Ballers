using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommand : DebugCommandBase
{

    private readonly Action<string[]> m_cmd;

    public Action<string[]> Command { get { return m_cmd; } }

    public DebugCommand(string name, string desc, string format, Action<string[]> cmd) : base(name, desc, format)
    {
        m_cmd = cmd;
    }

    public virtual bool Invoke(string cmdName, string[] split)
    {
        m_cmd.Invoke(split);
        return true;
    }

}
