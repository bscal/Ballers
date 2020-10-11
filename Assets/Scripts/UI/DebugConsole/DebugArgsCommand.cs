using System;
using System.Collections.Generic;

public class DebugArgsCommand : DebugCommand
{

    private readonly int m_argCount;

    public int ArgCount { get { return m_argCount; } }

    public DebugArgsCommand(string name, string desc, string formatted, Action<string[]> action, int argCount)
        : base(name, desc, formatted, action)
    {
        m_argCount = argCount;
    }

    public bool Invoke(string cmdName, List<string> args)
    {
        if (IsValid(cmdName, args))
        {
            Command.Invoke(args.ToArray());
            return true;
        }
        else
        {
            HandleError(cmdName, args);
        }
        return false;
    }

    public bool IsValid(string cmdName, List<string> args)
    {
        return args.Count == m_argCount;
    }

    public void HandleError(string cmdName, List<string> args)
    {
        HandleError($"Got ({args.Count}) args but expected ({ArgCount}).");
    }

}
