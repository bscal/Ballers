using Luminosity.IO;
using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        m_player.isMoving = IsMoving();
        m_player.isSprinting = Input.GetKey(KeyCode.LeftShift);

        m_player.isCtrlDown = InputManager.GetKey(KeyCode.LeftControl);
        m_player.isAltDown = InputManager.GetKey(KeyCode.LeftAlt);

        m_player.isDribUp = InputManager.GetKey(KeyCode.W);
        m_player.isDribDown = InputManager.GetKey(KeyCode.S);
        m_player.isDribLeft = InputManager.GetKey(KeyCode.A);
        m_player.isDribRight = InputManager.GetKey(KeyCode.D);

        if (Input.GetKey(KeyCode.Y) && !m_jumpCooldown)
        {
            m_animator.SetTrigger("Jump");
            StartCoroutine(WaitJump(1.5f));
        }

        TryPassBall();

//         if (Input.GetKey(KeyCode.Alpha1))
//         {
//             Player dummy = GameObject.Find("DummyPasser").GetComponent<Player>();
//             GameManager.GetBallHandling().TryPassBall(m_player, dummy, PassType.CHESS);
//         }

        if (Input.GetKeyDown(KeyCode.Escape))
            m_menu.SetActive(!m_menu.activeSelf);

        m_animator.SetBool("isDribbling", m_player.isDribbling);
        m_animator.SetBool("isSprinting", m_player.isSprinting);
        m_animator.SetBool("isWalking", m_player.isMoving);

        if (Input.GetKey(KeyCode.U)) m_animator.SetTrigger("Crossover");
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
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

    private void TryPassBall()
    {
        int passCode = 0;
        if (InputManager.GetButtonDown("pass_1"))
            passCode = 1;
        if (InputManager.GetButtonDown("pass_2"))
            passCode = 2;
        if (InputManager.GetButtonDown("pass_3"))
            passCode = 3;
        if (InputManager.GetButtonDown("pass_4"))
            passCode = 4;
        if (InputManager.GetButtonDown("pass_5"))
            passCode = 5;

        if (passCode != 0)
        {
            //Player dummy = GameObject.Find("DummyPasser").GetComponent<Player>();
            GameManager.GetBallHandling().TryPassBall(m_player, passCode, PassType.CHESS);
        }
    }

    IEnumerator ShotInput()
    {
        while (true)
        {
            if (m_shootCooldown || m_player.isShooting) yield return new WaitForSeconds(0.1f);

            //Check when the key is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Continue to check if it is still heldown and keep counting the how long
                while (Input.GetKey(KeyCode.Space))
                {
                    //Start incrementing timer
                    pressTimer += Time.deltaTime;

                    //Check if this counts as being "Held Down"
                    if (pressTimer > timeToCountAsHeldDown)
                    {
                        //It a "key held down", call the OnKeyHeldDown function and wait for it to return
                        yield return OnKeyHeldDown();
                        //No need to continue checking for Input.GetKey(KeyCode.D). Break out of this while loop
                        break;
                    }

                    //Wait for a frame
                    yield return null;
                }
            }


            //Check if key is released 
            if (Input.GetKeyUp(KeyCode.Space))
            {
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
            if (!Input.GetKey(KeyCode.Space)) break;

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




}
