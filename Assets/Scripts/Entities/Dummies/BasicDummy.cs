using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDummy : MonoBehaviour
{

    public enum DummyType
    {
        BASIC,
        SCREENER
    }

    [Header("Dummy Type")]
    [Tooltip("The type of dummy to create")]
    [SerializeField]
    private DummyType m_type;
    public DummyType Type { get { return m_type; } }

    private Player m_player;

    void Start()
    {
        m_player = GetComponent<Player>();

        switch (Type)
        {
            case DummyType.BASIC: break;
            case DummyType.SCREENER:
                m_player.isScreening = true;
                break;

            default: break;



        }
    }

    void Update()
    {
        
    }
}
