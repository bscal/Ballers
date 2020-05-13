using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsTrigger : MonoBehaviour
{

    private const string MAP_BOUNDS_TAG = "Bounds Trigger";

    private void OnTriggerStay(Collider other)
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                print("bounds");
                Player p = other.gameObject.GetComponent<Player>();
                if (p.HasBall && p.transform.position.y < 0.1f)
                    GameManager.Singleton.Turnover();
            }
        }
    }
}
