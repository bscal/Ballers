using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCamera : MonoBehaviour
{

    public Camera cam;
    public GameObject player;
    public GameObject ball;
    public GameObject basket;
    public Vector3 target;

    private bool m_isRotating = false;
    private int m_lastPossession = -1;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }

        ball = GameObject.Find("Ball");
    }

    void LateUpdate()
    {
        // Check if player is null
        if (player == null)
        {
            player = SpawnManager.GetLocalPlayerObject()?.gameObject;
            return;
        }

        // Check if there is a check of possession
        // If there is sets the new basket and to rotate


        if (m_lastPossession != GameManager.GetBallHandling().PossessionOrHome)
        {
            // Sets basket based on which team has possession of ball
            basket = GameManager.Singleton.baskets[GameManager.GetBallHandling().PossessionOrHome].gameObject;
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



        // Handles settings of position and applying proper offset
        float dist = Vector3.Distance(basket.transform.position, player.transform.position) * .4f;
        target.Set(0, 10, (GameManager.GetBallHandling().PossessionOrHome == 0) ? -dist : dist);
        cam.transform.position = Vector3.Lerp(cam.transform.position, target, 1.5f * Time.deltaTime);
        target = player.transform.position + basket.transform.position + ((ball != null) ? ball.transform.position : Vector3.zero);
    }
}