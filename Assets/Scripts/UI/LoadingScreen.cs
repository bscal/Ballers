using MLAPI.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{

    public GameObject loadingPanel;
    public TipData tipDate;
    public Text text;

    void Start()
    {
        loadingPanel = transform.GetChild(0).gameObject;
    }


    void OnEnable()
    {
        loadingPanel = transform.GetChild(0).gameObject;
        loadingPanel.SetActive(true);
        tipDate.index = Random.Range(0, tipDate.tips.Length);
        text.text = tipDate.tips[tipDate.index];
    }

    void OnDisable()
    {
        loadingPanel.SetActive(false);
    }
}
