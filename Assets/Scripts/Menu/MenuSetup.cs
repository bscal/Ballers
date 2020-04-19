using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSetup : MonoBehaviour
{

    public GameObject modelPrefab;
    private GameObject m_model;

    void Awake()
    {
        if (modelPrefab)
        {
            m_model = Instantiate(modelPrefab, Vector3.zero, Quaternion.identity);
            m_model.name = "Player";
        }
    }

    void Start()
    {
        
    }

}
