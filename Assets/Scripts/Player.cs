using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public bool IsRightHanded { get; set; }     = true;
    public bool IsDribbling { get; set; }       = false;
    public bool IsWalking { get; set; }         = false;
    public bool IsSprinting { get; set; }       = false;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", IsDribbling, IsWalking, IsSprinting), 0);

        IsWalking = IsMoving();
        IsSprinting = Input.GetKey(KeyCode.LeftShift);
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}
