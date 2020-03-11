using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    public GameObject obj;
    public Color color;

    void Start()
    {
        obj.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }
}
