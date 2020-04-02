using UnityEngine;

public class ThreePointCollision : MonoBehaviour
{

    private static string PLAYER_TAG = "Player";

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(PLAYER_TAG))
        {
            GameManager.GetPlayer().isInsideThree = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(PLAYER_TAG))
        {
            GameManager.GetPlayer().isInsideThree = false;
        }
    }
}
