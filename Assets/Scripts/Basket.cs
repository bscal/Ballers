using UnityEngine;

public class Basket : MonoBehaviour
{
    public bool isHome;
    public Transform netPos;

    private void Start()
    {
        if (!netPos)
            Debug.LogError("Basket needs to have a net position!");
    }
}
