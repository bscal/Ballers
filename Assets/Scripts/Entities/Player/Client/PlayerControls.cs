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
    private PlayerAnimHandler m_animHandler;
    private GameObject m_menu;

    private float m_shootCooldown = 0;
    private float m_jumpCooldown = 0;

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

        actions.Keyboard.Pass_1.canceled += ReleasePass;
        actions.Keyboard.Pass_2.canceled += ReleasePass;
        actions.Keyboard.Pass_3.canceled += ReleasePass;
        actions.Keyboard.Pass_4.canceled += ReleasePass;
        actions.Keyboard.Pass_5.canceled += ReleasePass;

        actions.Keyboard.Shoot.performed += StartShot;
        actions.Keyboard.Release.performed += StopShot;
        actions.Keyboard.Pumpfake.performed += Pumpfake;
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
        m_animHandler = GetComponent<PlayerAnimHandler>();
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
        m_player.movingFoward = dribVec.y > 0; //1
        m_player.movingBack = dribVec.y < 0; //-1
        m_player.movingLeft = dribVec.x < 0; //-1
        m_player.movingRight = dribVec.x > 0; //1

        if (actions.Keyboard.Jump.triggered && m_jumpCooldown < Time.time)
        {
            m_player.Jump();
            m_jumpCooldown = Time.time + 1.5f;
        }

        if (Keyboard.current.escapeKey.isPressed)
            m_menu.SetActive(!m_menu.activeSelf);

        if (Keyboard.current.uKey.isPressed) m_animator.SetTrigger("Crossover");
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
            PassType type = PassType.CHESS;
            if (Keyboard.current.leftShiftKey.ReadValue() > 0)
            {
                type = PassType.BOUNCE;
            }
            if (Keyboard.current.leftCtrlKey.ReadValue() > 0)
            {
                type = PassType.LOB;
            }
            GameManager.GetBallHandling().TryPassBall(m_player, passCode, type);
        }
    }

    private void ReleasePass(InputAction.CallbackContext context)
    {
        m_player.ReleasePass();
    }

    IEnumerator ShotInput()
    {
        bool triggered = false;
        bool held = false;
        while (true)
        {
            if (m_shootCooldown < Time.time || m_player.isShooting) yield return new WaitForSeconds(0.1f);

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

        //Pumpfake();

        yield return null;
    }


    IEnumerator OnKeyHeldDown()
    {

        //StartShot();

        //Move 1 unit every frame until edge detection is reached!
        while (pressTimer < MAX_TIME)
        {
            if (actions.Keyboard.Shoot.ReadValue<float>() == 0) break;

            //Start incrementing timer
            pressTimer += Time.deltaTime;

            //Wait for a frame
            yield return null;
        }

        //StopShot();
        pressTimer = 0f;
        yield return null;
    }

    void Pumpfake(InputAction.CallbackContext context)
    {
        if (m_shootCooldown < Time.time)
        {
            m_animHandler.Play(AnimNames.REG_PUMPFAKE);
            m_shootCooldown = Time.time + .2f;
        }
    }

    void StartShot(InputAction.CallbackContext context)
    {
        if (m_shootCooldown < Time.time)
        {
            m_player.ShootBall();
            m_shootCooldown = Time.time + .2f;
        }
    }

    void StopShot(InputAction.CallbackContext context)
    {
        if (m_shootCooldown < Time.time)
        {
            m_player.ReleaseBall();
            m_shootCooldown = Time.time + .2f;
        }
    }
}
