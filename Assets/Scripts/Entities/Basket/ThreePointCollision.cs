using MLAPI;
using MLAPI.Spawning;
using UnityEngine;

public class ThreePointCollision : MonoBehaviour
{

    private static string PLAYER_TAG = "Player";

    private void OnTriggerStay(Collider other)
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            if (other.gameObject.CompareTag(PLAYER_TAG))
            {
                Player p = other.gameObject.GetComponent<Player>();
                p.isInsideThree = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            if (other.gameObject.CompareTag(PLAYER_TAG))
            {
                Player p = other.gameObject.GetComponent<Player>();
                p.isInsideThree = false;
            }
        }
    }
}
