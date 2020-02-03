using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Debugger : MonoBehaviour
{
    public static Debugger Instance { get; set; }

    TextMeshProUGUI title;
    TextMeshProUGUI[] lines = new TextMeshProUGUI[4];

    GameObject debugPanel;

    void Start()
    {
        Instance = this;

        title = GameObject.Find("DebugTitle").GetComponent<TextMeshProUGUI>();
        debugPanel = GameObject.Find("DebugPanel");

        for (int i = 0; i < 4; i++)
        {
            lines[i] = GameObject.Find(string.Format("Debug ({0})", i)).GetComponent<TextMeshProUGUI>();
        }

        debugPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        title.text = string.Format("Debug T={0}, F={1}", Mathf.CeilToInt(Time.realtimeSinceStartup), Time.frameCount);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    public void Print(string param, int line)
    {
        lines[line].text = param;
    }
}
