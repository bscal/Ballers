using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{
    public float turnspeed;
    public float speed;
    public float upSpeed;

    float m_horizontal;
    float m_vertical;

    Camera m_camera;
    Movement m_movement;
    Controls m_controls;

    private void Awake()
    {
        m_controls = new Controls();
        m_controls.Enable();

        m_camera = GetComponent<Camera>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            Vector2 move = m_controls.Keyboard.Move.ReadValue<Vector2>();

            transform.position += new Vector3(move.x, 0, move.y) * speed * Time.deltaTime;

            if (Keyboard.current.leftShiftKey.ReadValue() > 0)
                transform.Translate(0.0f, upSpeed * Time.deltaTime, 0.0f);
            else if (Keyboard.current.leftCtrlKey.ReadValue() > 0)
                transform.Translate(0.0f, -upSpeed * Time.deltaTime, 0.0f);
        }
    }

    private void OnEnable()
    {
        if (GameManager.GetPlayer() == null)
            return;
        if (m_movement == null)
            m_movement = GameManager.GetPlayer().GetComponent<Movement>();
        if (m_movement != null)
            m_movement.isMovementEnabled = false;
    }

    private void OnDisable()
    {
        if (GameManager.GetPlayer() == null)
            return;
        if (m_movement == null)
            m_movement = GameManager.GetPlayer().GetComponent<Movement>();
        if (m_movement != null)
            m_movement.isMovementEnabled = true;
    }
}