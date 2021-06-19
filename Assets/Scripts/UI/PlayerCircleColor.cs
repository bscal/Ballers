using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCircleColor : MonoBehaviour
{

    private readonly static Color PLAYER_COLOR = Color.white;
    private readonly static Color ALLY_COLOR = Color.blue;
    private readonly static Color ENEMY_COLOR = Color.red;
    private readonly static Color DUMMY_COLOR = Color.gray;

    private void Start()
    {
        if (ServerManager.IS_DEDICATED_SERVER)
            Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.HasStarted) return;

        Player current = GameManager.GetPlayer();
        foreach (Player p in GameManager.GetPlayers())
        {
            if (p.Equals(current))
                current.SetCircleColor(PLAYER_COLOR);
            else if (p.props.teamID == current.props.teamID)
                p.SetCircleColor(ALLY_COLOR);
            else
                p.SetCircleColor(ENEMY_COLOR);
        }

        foreach (BasicDummy d in GameManager.GetDummies())
        {
            d.GetPlayer().SetCircleColor(DUMMY_COLOR);
        }
    }
}
