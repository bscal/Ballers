using MLAPI;
using UnityEngine;

public class BoundsTrigger : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameManager.Instance.OutOfBounds();
            }
            else if (other.gameObject.CompareTag("Ball"))
            {
                GameManager.Instance.OutOfBounds();
            }
        }
    }
}
