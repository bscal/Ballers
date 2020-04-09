using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUtils : MonoBehaviour
{

    private static Transform tempTrans =  new GameObject().transform;

    public static IEnumerator Dunk(Player p, Basket b)
    {
        //TODO MESSY MESSY MESSY REDO functionally works but needs to be cleaned up alot
        // current bug: lerps the position of the player not his hand
        p.isMovementFrozen = true;
        tempTrans.position = b.netPos.position;

        Vector3 startPos = p.transform.position;

        tempTrans.LookAt(p.transform);
        Vector3 tempV = new Vector3(0, tempTrans.rotation.eulerAngles.y, tempTrans.rotation.eulerAngles.z);
        tempTrans.rotation = Quaternion.Euler(tempV);
        tempTrans.position += tempTrans.forward * Basket.RADIUS;
        
        float startTime = Time.time;
        float fracComplete = 0;
        while (fracComplete < .99)
        {
            fracComplete = (Time.time - startTime) / .6f;
            p.transform.position = Vector3.Lerp(startPos, tempTrans.position, fracComplete);
            yield return null;
        }
        float height = p.transform.position.y - startPos.y;
        startTime = Time.time;
        fracComplete = 0;
        while (fracComplete < .99)
        {
            fracComplete = (Time.time - startTime) / .6f;
            p.transform.position = Vector3.Lerp(tempTrans.position, tempTrans.position - Vector3.up * height, fracComplete);
            yield return null;
        }

        p.isMovementFrozen = false;
    }

}
