using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //TODO REMOVE DEBUG
    private Debugger m_debugger;

    public bool IsRightHanded { get; set; }     = true;
    public bool IsDribbling { get; set; }       = false;
    public bool IsWalking { get; set; }         = false;
    public bool IsSprinting { get; set; }       = false;


    // Start is called before the first frame update
    void Start()
    {
        m_debugger = GameObject.Find("GameObject").GetComponent<Debugger>();
    }

    // Update is called once per frame
    void Update()
    {
        m_debugger.Print(string.Format("IsD: {0}, IsW: {1}, IsS: {2}", IsDribbling, IsWalking, IsSprinting), 0);

        IsWalking = IsMoving();
        IsSprinting = Input.GetKey(KeyCode.LeftShift);
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}
