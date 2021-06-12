using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public GameObject handObject;

    public Hand(GameObject handObject)
    {
        this.handObject = handObject;
    }

}
