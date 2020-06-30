using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIExitButton : MonoBehaviour
{

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

}
