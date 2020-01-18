using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;

public class ThreePointCollision : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "3pt Lines" && !GameManager.GetPlayer().isInsideThree)
        {
            print(1);
            GameManager.GetPlayer().isInsideThree = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "3pt Lines" && GameManager.GetPlayer().isInsideThree)
        {
            print(2);
            GameManager.GetPlayer().isInsideThree = false;
        }
    }
}
