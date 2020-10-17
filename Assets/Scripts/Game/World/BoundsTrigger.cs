using MLAPI;
using UnityEngine;

public class BoundsTrigger : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameManager.Singleton.OutOfBounds();
            }
            else if (other.gameObject.CompareTag("Ball"))
            {
                GameManager.Singleton.OutOfBounds();
            }
        }
    }
}
