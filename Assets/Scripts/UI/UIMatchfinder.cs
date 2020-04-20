using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMatchfinder : MonoBehaviour
{

    private const string FINDING_TEXT = "Finding Match ";
    private static readonly string[] DOTS = { ".  ", ".. ", "..." };

    public Matchmaking matchmaking;
    public Text text;

    private int dotCount = 2;

    public void Hide()
    {
        LeanTween.scaleX(gameObject, 0f, .4f);
        matchmaking.StopFinding();
    }

    public void Unhide()
    {
        LeanTween.scaleX(gameObject, 1f, .4f);
        matchmaking.StartFinding();
        StartCoroutine(UpdateText());
    }

    private IEnumerator UpdateText()
    {
        while (matchmaking.IsFinding)
        {
            yield return new WaitForSeconds(.5f);
            dotCount++;
            if (dotCount > 2) dotCount = 0;
            text.text = FINDING_TEXT + DOTS[dotCount];
        }
    }
}
