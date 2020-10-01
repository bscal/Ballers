using UnityEngine;

public class Basket : MonoBehaviour
{

    public const float RADIUS = .5f;

    public uint id = 0;
    public Transform netPos;
    public Transform bottomOfNet;
    public Transform floorUnderNet;
    public Cloth netCloth;
    public GameObject[] banks;
    private void Start()
    {
        if (!netPos)
            Debug.LogError("Basket needs to have a net position!");
    }
}