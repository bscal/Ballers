using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkedBehaviour
{

    private const float MAX_TIME = 3.0f;
    private const float timeToCountAsHeldDown = 0.3f;
    private float pressTimer = 0;

    private Player m_player;
    private Animator m_animator;
    private GameObject m_menu;

    private bool m_shootCooldown = false;
    private bool m_jumpCooldown = false;

    private Controls actions;

    private void OnEnable()
    {
        actions = new Controls();
        actions.Enable();

        actions.Keyboard.Pass_1.started += TryPassBall;
        actions.Keyboard.Pass_2.started += TryPassBall;
        actions.Keyboard.Pass_3.started += TryPassBall;
        actions.Keyboard.Pass_4.started += TryPassBall;
        actions.Keyboard.Pass_5.started += TryPassBall;
        actions.Keyboard.Callforball.started += CallForBall;
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    void Start()
    {
        if (!IsOwner)
            return;

        m_player = GetComponent<Player>();
        m_animator = GetComponentInChildren<Animator>();
        m_menu = GameObject.Find("Menu Panel");
        StartCoroutine(ShotInput());
    }

    void Update()
    {
        if (!IsOwner)
            return;

        m_player.isSprinting = Keyboard.current.shiftKey.ReadValue() > 0.0f;

        m_player.isCtrlDown = Keyboard.current.ctrlKey.ReadValue() > 0.0f;
        m_player.isAltDown = Keyboard.current.altKey.ReadValue() > 0.0f;


        Vector2 dribVec = actions.Keyboard.Dribble.ReadValue<Vector2>();
        m_player.isDribUp = dribVec.y > 0; //1
        m_player.isDribDown = dribVec.y < 0; //-1
        m_player.isDribLeft = dribVec.x < 0; //-1
        m_player.isDribRight = dribVec.x > 0; //1

        if (actions.Keyboard.Jump.triggered && !m_jumpCooldown)
        {
            m_animator.SetTrigger("Jump");
            StartCoroutine(WaitJump(1.5f));
        }


        //TryPassBall();

//         if (IsKeyPressed(actions.Keyboard.Callforball))
//         {
//             if (!m_player.HasBall && m_player.IsOnOffense())
//             {
//                 m_player.CallForBall();
//             }
//         }

        if (Keyboard.current.escapeKey.isPressed)
            m_menu.SetActive(!m_menu.activeSelf);

        m_animator.SetBool("isDribbling", m_player.isDribbling);
        m_animator.SetBool("isSprinting", m_player.isSprinting);
        m_animator.SetBool("isWalking", m_player.isMoving);

        if (Keyboard.current.uKey.isPressed) m_animator.SetTrigger("Crossover");
    }

    private IEnumerator WaitShoot(float delay)
    {
        m_shootCooldown = true;
        yield return new WaitForSeconds(delay);
        m_shootCooldown = false;
    }

    private IEnumerator WaitJump(float delay)
    {
        m_jumpCooldown = true;
        yield return new WaitForSeconds(delay);
        m_jumpCooldown = false;
    }

    private void CallForBall(InputAction.CallbackContext context)
    {
        if (!m_player.HasBall && m_player.IsOnOffense())
        {
            m_player.CallForBall();
        }
    }

    private void TryPassBall(InputAction.CallbackContext context)
    {
        int passCode = 0;
        if (context.action.name == "Pass_1")
            passCode = 1;
        else if (context.action.name == "Pass_2")
            passCode = 2;
        else if(context.action.name == "Pass_3")
            passCode = 3;
        else if(context.action.name == "Pass_4")
            passCode = 4;
        else if(context.action.name == "Pass_5")
            passCode = 5;
        if (passCode != 0)
        {
            GameManager.GetBallHandling().TryPassBall(m_player, passCode, PassType.CHESS);
        }
    }

    IEnumerator ShotInput()
    {
        bool triggered = false;
        bool held = false;
        while (true)
        {
            if (m_shootCooldown || m_player.isShooting) yield return new WaitForSeconds(0.1f);

            //Check when the key is pressed
            if (actions.Keyboard.Shoot.ReadValue<float>() > 0)
            {
                triggered = true;
                held = true;
                //Continue to check if it is still helddown and keep counting the how long
                while (held)
                {
                    //Start incrementing timer
                    pressTimer += Time.deltaTime;

                    //Check if this counts as being "Held Down"
                    if (pressTimer > timeToCountAsHeldDown)
                    {
                        //It a "key held down", call the OnKeyHeldDown function and wait for it to return
                        yield return OnKeyHeldDown();
                        //Press has been handled
                        triggered = false;
                        held = false;
                        //No need to continue checking for Input.GetKey(KeyCode.D). Break out of this while loop
                        break;
                    }

                    if (actions.Keyboard.Shoot.ReadValue<float>() == 0)
                        held = false;

                    //Wait for a frame
                    yield return null;
                }
            }

            //Check if key is released 
            if (triggered && !held)
            {
                //Press has been handled
                triggered = false;
                //Check if we have not not reached the timer then it is only a key press
                if (pressTimer < timeToCountAsHeldDown)
                {
                    //It just a key press, call the OnKeyPressedOnly function and wait for it to return
                    yield return OnKeyPressedOnly();
                }

                //Reset timer to 0 for the next key press
                pressTimer = 0f;
            }

            //Wait for a frame
            yield return null;
        }
    }

    IEnumerator OnKeyPressedOnly()
    {

        Pumpfake();

        yield return null;
    }


    IEnumerator OnKeyHeldDown()
    {

        StartShot();

        //Move 1 unit every frame until edge detection is reached!
        while (pressTimer < MAX_TIME)
        {
            if (actions.Keyboard.Shoot.ReadValue<float>() == 0) break;

            //Start incrementing timer
            pressTimer += Time.deltaTime;

            //Wait for a frame
            yield return null;
        }

        StopShot();
        pressTimer = 0f;
        yield return null;
    }

    void Pumpfake()
    {
        m_animator.SetTrigger("Pumpfake");
        StartCoroutine(WaitShoot(0.20f));
    }

    void StartShot()
    {
        m_player.ShootBall();
        //m_animator.SetTrigger("Shoot");
        StartCoroutine(WaitShoot(0.20f));
    }

    void StopShot()
    {
        m_player.ReleaseBall();
        m_animator.ResetTrigger("Shoot");
        StartCoroutine(WaitShoot(0.20f));
    }

    private static bool IsKeyPressed(InputAction action)
    {
        return action.ReadValue<float>() > 0;
    }



}
