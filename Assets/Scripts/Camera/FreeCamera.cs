using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float turnspeed;
    public float speed;
    public float upSpeed;

    float m_horizontal;
    float m_vertical;

    Camera m_camera;
    Movement m_movement;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }

        m_camera = GetComponent<Camera>();
        m_movement = SpawnManager.GetLocalPlayerObject()?.GetComponentInChildren<Movement>();
    }

    void Update()
    {
        if (m_movement == null)
            m_movement = SpawnManager.GetLocalPlayerObject()?.GetComponentInChildren<Movement>();

        if (m_camera.enabled)
        {
            m_horizontal = Input.GetAxis("Horizontal") * (turnspeed * Time.deltaTime);
            transform.Rotate(0f, m_horizontal, 0f);
            m_vertical = Input.GetAxis("Vertical") * (speed * Time.deltaTime);
            transform.Translate(0.0f, 0.0f, m_vertical);

            if (Input.GetKey(KeyCode.Space))
                transform.Translate(0.0f, upSpeed * Time.deltaTime, 0.0f);
            else if (Input.GetKey(KeyCode.LeftShift))
                transform.Translate(0.0f, -upSpeed * Time.deltaTime, 0.0f);
        }
    }

    private void OnDisable()
    {
        m_movement.isEnabled = true;
    }
}