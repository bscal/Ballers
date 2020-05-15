using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestTracker : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Singleton.PlayerLoaded += OnPlayerLoaded;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnPlayerLoaded()
    {

    }
}
