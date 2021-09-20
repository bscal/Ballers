using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCamera : MonoBehaviour
{

    public Camera cam;
    public GameObject basket;
    public Vector3 target;

    private bool m_isRotating = false;
    private int m_lastPossession = -1;

    void Start()
    {
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
        {
            Destroy(this);
        }
    }

    void LateUpdate()
    {
        Player p = ClientPlayer.Instance.localPlayer;
        if (p && p.clientControlsEnabled && GameManager.Instance != null)
        {
            if (m_lastPossession != (int)GameManager.Instance.TeamWithPossession.Value)
            {
                // Does the camera need to rotate?
                m_isRotating = true;
            }

            int basketId = (int)GameManager.Instance.TeamWithPossession.Value;
            if (basketId == -1)
                basketId = 0;
            
            basket = GameManager.Instance.baskets[basketId].gameObject;

            if (m_isRotating)
            {
                float step = 400.0f * Time.deltaTime;
                Vector3 v = basket.transform.rotation.eulerAngles;
                //basket.transform.eulerAngles = v;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(v.x + 15f, v.y, v.z), step);
                if (transform.rotation == basket.transform.rotation) m_isRotating = false;
            }

            Transform playerTransform = p.transform;
            // Handles settings of position and applying proper offset
            float dist = Vector3.Distance(basket.transform.position, playerTransform.position) * .4f;
            target.Set(0, 10, (basketId == 0) ? -dist : dist);
            cam.transform.position = Vector3.Lerp(cam.transform.position, target, 1.5f * Time.deltaTime);
            target = playerTransform.position + basket.transform.position + GameManager.Instance.BallPosition.Value;
        }
    }
}
