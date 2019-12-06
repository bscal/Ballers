using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public int id;
    public string username = "test";
    public Dictionary<string, int> skills = new Dictionary<string, int>();

    public bool IsRightHanded { get; set; }    = true;
    public bool IsDribbling { get; set; }      = false;
    public bool IsMoving { get; set; }         = false;
    public bool IsSprinting { get; set; }      = false;


    // Start is called before the first frame update
    void Start()
    {
        id = username.GetHashCode();
    }

    // Update is called once per frame
    void Update()
    {
        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", IsDribbling, IsMoving, IsSprinting), 0);
    }

}
