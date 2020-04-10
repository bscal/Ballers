using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUtils : MonoBehaviour
{

    private static readonly Transform m_tempTrans =  new GameObject().transform;

    public static IEnumerator Dunk(Player p, Basket b)
    {
        p.isMovementFrozen = true;

        m_tempTrans.position = b.netPos.position;
        m_tempTrans.LookAt(p.transform);
        Vector3 tempV = new Vector3(0, m_tempTrans.rotation.eulerAngles.y, m_tempTrans.rotation.eulerAngles.z);
        m_tempTrans.rotation = Quaternion.Euler(tempV);
        m_tempTrans.position += m_tempTrans.forward * Basket.RADIUS;

        Vector3 hand = p.GetHand;
        float startTime = Time.time;
        float fracComplete = 0;
        while (fracComplete < .99)
        {
            fracComplete = (Time.time - startTime) / .33f;
            p.transform.position = Vector3.Lerp(hand, m_tempTrans.position, fracComplete) - (hand - p.transform.position);
            yield return null;
        }

        startTime = Time.time;
        fracComplete = 0;
        Vector3 startPos = p.transform.position;
        Vector3 endPos = p.transform.position;
        endPos.y = .1f;
        while (fracComplete < .99)
        {
            fracComplete = (Time.time - startTime) / .6f;
            p.transform.position = Vector3.Lerp(startPos, endPos, fracComplete);
            yield return null;
        }
        p.transform.position = endPos;

        p.isMovementFrozen = false;
    }

}
