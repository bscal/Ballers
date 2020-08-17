using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDummy : MonoBehaviour
{

    public enum DummyType
    {
        BASIC,
        SCREENER,
        PASSER
    }

    [Header("Dummy Type")]
    [Tooltip("The type of dummy to create")]
    [SerializeField]
    protected DummyType m_type;
    public DummyType Type { get { return m_type; } }

    protected Player m_player;

    protected void Awake()
    {
        m_player = GetComponent<Player>();
        m_player.TeamID = (int)TeamType.AWAY;
    }

    protected void Start()
    {
        GameManager.AddDummy(this);

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

    public Player GetPlayer()
    {
        return m_player;
    }

}
