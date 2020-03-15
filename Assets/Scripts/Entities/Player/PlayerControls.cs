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

    private bool m_shootCooldown = false;
    private bool m_jumpCooldown = false;

    private Coroutine m_ShotInput;

   void Start()
   {
        m_player = GetComponent<Player>();
        m_animator = GetComponentInChildren<Animator>();
        m_ShotInput = StartCoroutine(ShotInput());
   }

    void Update()
    {
        if (!IsOwner || IsServer && !IsHost)
            return;

        if (GameManager.Singleton.HasStarted)
        {

        }

        m_player.isMoving = IsMoving();
        m_player.isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.Space) && !m_jumpCooldown)
        {
            m_animator.SetTrigger("Jump");
            StartCoroutine(WaitJump(1.5f));
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            Player dummy = GameObject.Find("DummyPasser").GetComponent<Player>();
            GameManager.GetBallHandling().TryPassBall(m_player, dummy, PassType.CHESS);
        }
        

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


    IEnumerator ShotInput()
    {
        while (true)
        {
            if (m_shootCooldown || m_player.isShooting) yield return new WaitForSeconds(0.1f);

            //Check when the key is pressed
            if (Input.GetKeyDown(KeyCode.Y))
            {
                //Continue to check if it is still heldown and keep counting the how long
                while (Input.GetKey(KeyCode.Y))
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
            if (Input.GetKeyUp(KeyCode.Y))
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
            if (!Input.GetKey(KeyCode.Y)) break;

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
        m_animator.SetTrigger("Shoot");
        StartCoroutine(WaitShoot(0.20f));
    }

    void StopShot()
    {
        m_player.ReleaseBall();
        m_animator.ResetTrigger("Shoot");
        StartCoroutine(WaitShoot(0.20f));
    }




}
