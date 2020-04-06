using UnityEngine;

public class Basket : MonoBehaviour
{

    public uint id = 0;
    public Transform netPos;
    public GameObject leftBank;
    public GameObject rightBank;

    private void Start()
    {
        if (!netPos)
            Debug.LogError("Basket needs to have a net position!");
    }
}