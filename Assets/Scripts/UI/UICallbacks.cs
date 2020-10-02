using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICallbacks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlayerReadyUp()
    {
        GameManager.GetPlayer().SetReadyStatus(true);
    }
}
