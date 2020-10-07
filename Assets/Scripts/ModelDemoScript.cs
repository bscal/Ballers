using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDemoScript : MonoBehaviour
{

    public GameObject leftHandPos;
    public GameObject rightHandPos;
    public GameObject ballObject;

    public Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ballObject.transform.position = rightHandPos.transform.position;
    }
}
