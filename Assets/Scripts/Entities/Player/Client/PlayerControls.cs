using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : NetworkBehaviour
{

    private const float MAX_TIME = 3.0f;
    private const float timeToCountAsHeldDown = 0.3f;
    private float pressTimer = 0;

    [SerializeField]
    private Player m_player;
    [SerializeField]
    private ClientNetworkHandler networkHandler;
    [SerializeField]
    private PlayerAnimHandler m_animHandler;
    private Animator m_animator;
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
        m_animator = GetComponentInChildren<Animator>();
        m_menu = GameObject.Find("Menu Panel");
        //StartCoroutine(ShotInput());
    }

    void Update()
    {
        if (IsOwner)
        {
            m_shootCooldown -= Time.deltaTime;

            m_player.props.isSprinting = Keyboard.current.shiftKey.ReadValue() > 0.0f;
            m_player.props.isCtrlDown = Keyboard.current.ctrlKey.ReadValue() > 0.0f;
            m_player.props.isAltDown = Keyboard.current.altKey.ReadValue() > 0.0f;

            Vector2 dribVec = actions.Keyboard.Dribble.ReadValue<Vector2>();
            m_player.props.movingFoward = dribVec.y > 0; //1
            m_player.props.movingBack = dribVec.y < 0; //-1
            m_player.props.movingLeft = dribVec.x < 0; //-1
            m_player.props.movingRight = dribVec.x > 0; //1

            if (actions.Keyboard.Jump.triggered && m_jumpCooldown < Time.time)
            {
                m_player.Jump();
                m_jumpCooldown = Time.time + 1.5f;
            }

            if (Keyboard.current.escapeKey.isPressed)
                m_menu.SetActive(!m_menu.activeSelf);

            if (Keyboard.current.uKey.isPressed) m_animator.SetTrigger("Crossover");
        }
    }

    private void CallForBall(InputAction.CallbackContext context)
    {
        if (m_player.CanDoAction() && m_player.IsOnOffense())
        {
            networkHandler.CallForBall();
        }
    }

    private void TryPassBall(InputAction.CallbackContext context)
    {
        if (m_player.CanDoAction())
        {
            int passCode = 0;
            if (context.action.name == "Pass_1")
                passCode = 1;
            else if (context.action.name == "Pass_2")
                passCode = 2;
            else if (context.action.name == "Pass_3")
                passCode = 3;
            else if (context.action.name == "Pass_4")
                passCode = 4;
            else if (context.action.name == "Pass_5")
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
                networkHandler.TryPassBall(m_player, passCode, type);
            }
        }

    }

    private void ReleasePass(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            m_player.ReleasePass();
        }
    }

    IEnumerator ShotInput()
    {
        bool triggered = false;
        bool held = false;
        while (true)
        {
            m_shootCooldown -= Time.deltaTime;
            if (m_shootCooldown <= 0f || m_player.props.isShooting) yield return new WaitForSeconds(0.1f);

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
        while (pressTimer < MAX_TIME && m_player.props.isShooting)
        {
            if (actions.Keyboard.Shoot.ReadValue<float>() == 0) break;
            pressTimer += Time.deltaTime;
            yield return null;
        }
        pressTimer = 0f;
        yield return null;
    }

    void Pumpfake(InputAction.CallbackContext context)
    {
        if (IsOwner && m_shootCooldown <= 0f)
        {
            m_player.PumpfakeServerRpc();
            m_animHandler.Play(AnimNames.REG_PUMPFAKE);
            m_shootCooldown = .2f;
        }
    }

    void StartShot(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (!m_player.props.isShooting && m_shootCooldown <= 0f && m_player.HasBall)
            {
                m_shootCooldown = .2f;
                m_player.ShootBall();
            }
        }
    }

    void StopShot(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (m_player.props.isShooting)
                m_player.ReleaseBall();
        }
    }
}
