﻿using Luminosity.IO;
using MLAPI;
using MLAPI.Spawning;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    public const float AUTO_TURN_SPEED = 3.0f;

    public Animator animator;

    public bool isEnabled = true;

    private float m_horizontal;
    private float m_vertical;
    private float m_strafe;

    private Player m_player;
    private GameObject m_parent;
    private Vector3 m_targetDirection;
    private bool m_skipRotate = false;

    private readonly float m_movementSpeed  = 8.0f;
    private readonly float m_sprintSpeed    = 16.0f;
    private readonly float m_turningSpeed   = 200.0f;

    private Controls actions;

    private void OnEnable()
    {
        actions = new Controls();
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }

        m_player = GetComponentInParent<Player>();
        m_parent = m_player.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_player.IsOwner) return;
        if (m_player.isMovementFrozen || !isEnabled) return;

        int possesion = GameManager.GetBallHandling().PossessionOrHome;

        if (m_player.HasBall && possesion != -1)
        {
            m_targetDirection = GameManager.Singleton.baskets[possesion].gameObject.transform.position - m_parent.transform.position;
        }
        else if (m_player.Assignment)
        {
            m_targetDirection = m_player.Assignment.transform.position - m_parent.transform.position;
        }
        else
        {
            m_targetDirection = GameManager.Singleton.baskets[possesion].gameObject.transform.position - m_parent.transform.position;
        }


        //         if (m_player.isMoving)
        //         {
        //             if (m_player.isSprinting)
        //             {
        //                 m_horizontal = Input.GetAxis("Horizontal") * (m_turningSpeed * Time.deltaTime);
        //                 m_parent.transform.Rotate(0f, m_horizontal, 0f);
        //                 m_vertical = Input.GetAxis("Vertical") * (m_sprintSpeed * Time.deltaTime);
        //                 m_strafe = 0f;
        //             }
        //             else
        //             {
        //                 if (m_targetDirection != null)
        //                 {
        //                     m_targetDirection.y = 0f;
        //                     // Rotate the forward vector towards the target direction by one step
        //                     Vector3 newDirection = Vector3.RotateTowards(m_parent.transform.forward, m_targetDirection, AUTO_TURN_SPEED * Time.deltaTime, 0.0f);
        //                     // Calculate a rotation a step closer to the target and applies rotation to this object
        //                     m_parent.transform.rotation = Quaternion.LookRotation(newDirection);
        //                 }
        //                 m_vertical = Input.GetAxis("Vertical") * (m_movementSpeed * Time.deltaTime);
        //                 m_strafe = Input.GetAxis("Horizontal") * (m_movementSpeed * Time.deltaTime);
        //                 if (Input.GetAxis("Horizontal") > 0)
        //                 {
        //                     //animator.CrossFade("strafeRight");
        //                 }
        //                 else if (Input.GetAxis("Horizontal") < 0)
        //                 {
        //                     //animator.CrossFade("strafeLeft");
        //                 }
        //             }
        //             m_parent.transform.Translate(m_strafe, 0f, m_vertical);
        //         }
    MovePlayer();
    RotatePlayer();
    /*
            else if (!m_skipRotate)
            {
                m_targetDirection.y = 0f;
                // Rotate the forward vector towards the target direction by one step
                Vector3 newDirection = Vector3.RotateTowards(m_parent.transform.forward, m_targetDirection, AUTO_TURN_SPEED * Time.deltaTime, 0.0f);
                // Calculate a rotation a step closer to the target and applies rotation to this object
                m_parent.transform.rotation = Quaternion.LookRotation(newDirection);

            }*/
    m_skipRotate = false;
    }

    private void MovePlayer()
    {
        float ms = (m_player.isSprinting ? m_sprintSpeed : m_movementSpeed) * Time.deltaTime;
        Vector3 mov = (GameManager.Singleton.Possession == 0 ? Vector3.forward : -Vector3.forward) * ms;

        Vector2 move = actions.Keyboard.Move.ReadValue<Vector2>();
        m_player.isMoving = move != Vector2.zero;

        m_player.isDribUp = move.y > 0;    //1
        m_player.isDribDown = move.y < 0;  //-1
        m_player.isDribLeft = move.x < 0;  //-1
        m_player.isDribRight = move.x > 0; //1

        if (m_player.isDribUp)
        {
            m_parent.transform.position += mov;
        }
        else if (m_player.isDribDown)
        {
            m_parent.transform.position -= mov;
        }

        mov = (GameManager.Singleton.Possession == 0 ? Vector3.left : -Vector3.left) * ms;
        if (m_player.isDribLeft)
        {
            m_parent.transform.position += mov;
        }
        else if (m_player.isDribRight)
        {
            m_parent.transform.position -= mov;
        }
    }

    private void RotatePlayer()
    {
        if (m_targetDirection != null)
        {
            m_targetDirection.y = 0f;
            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(m_parent.transform.forward, m_targetDirection, AUTO_TURN_SPEED * Time.deltaTime, 0.0f);
            // Calculate a rotation a step closer to the target and applies rotation to this object
            m_parent.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private void StrafePlayer()
    {

    }
}
