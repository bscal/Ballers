using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerPosition : Enumeration
{
    public readonly static BallerPosition PG = new BallerPosition(0, "PG");
    public readonly static BallerPosition SG = new BallerPosition(0, "SG");
    public readonly static BallerPosition SF = new BallerPosition(0, "SF");
    public readonly static BallerPosition PF = new BallerPosition(0, "PF");
    public readonly static BallerPosition C = new BallerPosition(0, "C");

    public BallerPosition(int id, string name)
        : base(id, name)
    {

    }

}
