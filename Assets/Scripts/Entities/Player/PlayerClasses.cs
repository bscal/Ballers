using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerClass
{

    ALL_AROUND = 0,
    PLAY_MAKER = 1

}

static class PlayerClassExtensions
{

    private static readonly IReadOnlyList<string> m_idToNameMap = new List<string>()
    { 
        "All Around",
        "Player Maker"
    };

    public static string GetName(PlayerClass pc)
    {
        return m_idToNameMap[(int) pc];
    }

}
