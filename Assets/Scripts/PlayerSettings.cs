using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{

    public Camera main;
    public Camera side;

    // Start is called before the first frame update
    void Start()
    {
        main.enabled = true;
        side.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            main.enabled = !main.enabled;
            side.enabled = !side.enabled;
        }
    }
}
