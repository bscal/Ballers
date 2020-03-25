using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [Header("Name")]
    public Text charName;

    [Header("Character")]
    public Text position;
    public Text height;
    public Text weight;
    public Text wingspan;

    [Header("Offense")]
    public Text close;
    public Text midRange;
    public Text Three;

    public static string FormatHeight(int height)
    {
        return string.Format("{0}\"{1}\'", Mathf.RoundToInt(height/12), height%12);
    }

    public static string FormatWeight(int weight)
    {
        return string.Format("{0}lbs", weight);
    }

    public static string FormatName(string first, string last)
    {
        return string.Format("{0} {1}", first, last);
    }

    public static string FormatPos(int pos)
    {
        string result;
        switch (pos)
        {
            case 1:
                result = "PG";
                break;
            case 2:
                result = "SG";
                break;
            case 3:
                result = "SF";
                break;
            case 4:
                result = "PF";
                break;
            default:
                result = "C";
                break;
        }
        return result;
    }

}
