using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTriggers : MonoBehaviour
{

    private Basket m_basket;

    private void Awake()
    {
        m_basket = GetComponentInParent<Basket>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (gameObject.name.Equals("Hitbox Top"))
        {
            //GameManager.GetBallHandling().hitTopTrigger = true;
        }

        if (gameObject.name.Equals("Hitbox Bot") && other.gameObject.CompareTag("Ball"))
        {
            Debug.Break();
            Vector3 dir = (transform.position - other.transform.position).normalized;
            // Detect if coming from above the collider.
            GameManager.GetBallHandling().OnShotMade((int)m_basket.id);
            m_basket.netCloth.externalAcceleration = new Vector3() {
                x = UnityEngine.Random.Range(5, 12),
                y = UnityEngine.Random.Range(32, 48),
                z = UnityEngine.Random.Range(5, 12),
            };
            LeanTween.delayedCall(.5f, () => m_basket.netCloth.externalAcceleration = Vector3.zero);
            if (dir.y > 0)
            {

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (gameObject.name.Equals("Hitbox Shot") && GameManager.GetBallHandling().shotInAction)
        {
            if (!other.bounds.Contains(GameManager.GetBallHandling().gameObject.transform.position))
            {
                GameManager.GetBallHandling().OnShotMissed();
            }
        }
    }
}
