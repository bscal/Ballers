using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDifficulty : Enumeration
{
    public readonly static AIDifficulty ROOKIE = new AIDifficulty(0, "Rookie", .75f);
    public readonly static AIDifficulty PRO = new AIDifficulty(1, "Pro", 1.0f);
    public readonly static AIDifficulty ALL_STAR = new AIDifficulty(2, "All Star", 1.25f);

    public float Modifier { get; private set; }

    AIDifficulty(int id, string name, float modifier)
        : base(id, name)
    {
        Modifier = modifier;
    }

}
