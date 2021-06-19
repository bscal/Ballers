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
        if (p && p.clientControlsEnabled)
        {
            if (m_lastPossession != GameManager.Instance.ballController.PossessionOrHome)
            {
                // Sets basket based on which team has possession of ball
                basket = GameManager.Instance.baskets[GameManager.Instance.ballController.PossessionOrHome].gameObject;
                // Does the camera need to rotate?
                m_isRotating = true;
            }

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
            target.Set(0, 10, (GameManager.Instance.ballController.PossessionOrHome == 0) ? -dist : dist);
            cam.transform.position = Vector3.Lerp(cam.transform.position, target, 1.5f * Time.deltaTime);
            target = playerTransform.position + basket.transform.position + GameManager.Instance.ball.transform.position;
        }
    }
}
