using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Tips", menuName = "ScriptableObjects/TipData", order = 1)]
public class TipData : ScriptableObject
{

    public int index;
    public string[] tips;

}
