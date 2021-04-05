using UnityEngine;
using MLAPI;
using MLAPI.Connection;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;

[Serializable]
public enum BallState
{
    NONE,
    LOOSE,
    HELD,
    PASS,
    JUMPBALL,
    SHOT,
    INBOUND,
    DEAD_BALL
}

[Serializable]
public enum PassType
{
    CHESS,
    BOUNCE,
    LOB,
    FLASHY,
    ALLEY_OOP
}

public class BallHandling : NetworkBehaviour
{
    // =================================== Constants ===================================

    public const ulong NO_PLAYER = ulong.MaxValue;
    public const ulong DUMMY_PLAYER = ulong.MaxValue - 1;

    [SerializeField]
    private const float SHOT_SPEED = 0.75f;

    // =================================== Public Events ===================================

    public event Action<int, ShotData, ShotResultData> ShotMade;
    public event Action<ShotData, ShotResultData> ShotMissed;
    public event Action<BallState> BallStateChange;
    public event Action<int> PossessionChange;
    public event Action<ulong, ulong> BallHandlerChange;
    public event Action<Player, Player, float, PassType> PassBall;
    public event Action<Player, PassType> CatchBall;
    public event Action<Player, float> TouchedBall;

    // =================================== Public Networking Variables ===================================

    // PlayerID with ball
    private readonly NetworkVariableULong m_playerWithBall = new NetworkVariableULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerWithBall { get { return m_playerWithBall.Value; } set { m_playerWithBall.Value = (value); } }

    // PlayerID last touched ball
    private readonly NetworkVariableULong m_playerLastTouched = new NetworkVariableULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerLastTouched { get { return m_playerLastTouched.Value; } set { m_playerLastTouched.Value = (value); } }

    // PlayerID last possession
    private readonly NetworkVariableULong m_playerLastPossesion = new NetworkVariableULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerLastPossesion { get { return m_playerLastPossesion.Value; } set { m_playerLastPossesion.Value = (value); } }

    // BallState
    private readonly NetworkVariableByte m_state = new NetworkVariableByte(NetworkConstants.BALL_CHANNEL, 0);
    public BallState State { get { return (BallState) Enum.ToObject(typeof(BallState), m_state.Value); } set { m_state.Value = (byte)value; BallStateChange?.Invoke(value); } }

    // TeamID with possession
    private readonly NetworkVariableSByte m_possession = new NetworkVariableSByte(NetworkConstants.BALL_CHANNEL, -1);
    public int Possession { get { return m_possession.Value; } set { if (value < -1 || value > 1) value = -1; m_possession.Value = (sbyte)value; } }
    public int PossessionOrHome { get { return (Possession != 1) ? 0 : 1; } }

    // =================================== Public ===================================

    public float pass_speed = 6.0f;
    public float walk_offset = 2.5f;
    public float sprint_offset = 6.0f;

    // =================================== Private ===================================

    private ShotManager m_shotManager;
    private Player m_currentPlayer;
    private GameObject m_ball;
    private Rigidbody m_body;
    private SphereCollider m_collider;

    private ShotData m_shotData;
    private ShotBarData m_shotBarData;

    private ulong m_shooter;
    private int m_grade;
    private float m_timer;
    private float m_passScore;
    private float m_releaseDiff;
    private float m_shotDifficulty;
    private bool m_hitRim;
    private bool m_isAboveRim;
    private bool m_shotPastArc;
    public bool hitTopTrigger;
    public bool shotInAction;
    private bool m_collisionCheck;
    private bool m_insideHitboxShot;

    private Vector3 pos;
    private float maxDistance;
    private bool moved;
    private bool breakShot;

    private Dictionary<ulong, float> m_playerDistances;
    private IOrderedEnumerable<KeyValuePair<ulong, float>> m_pairs;

    // =================================== Functions ===================================

    private void Awake()
    {
        if (IsServer)
        {
            GameManager.Singleton.GameStartedServer += OnGameStarted;
            BallHandlerChange += OnChangeBallHandler;
        }
    }

    public override void NetworkStart()
    {
        GameManager.SetBallHandlingInstance(this);
        m_shotManager = GameManager.Singleton.gameObject.GetComponent<ShotManager>();
        m_ball = NetworkObject.gameObject;
        m_collider = GetComponent<SphereCollider>();
        name = "Ball";
        if (IsServer)
        {
            m_playerDistances = new Dictionary<ulong, float>();
            m_body = gameObject.GetComponent<Rigidbody>();
            m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (IsServer && Match.HasGameStarted)
        {
            m_timer += Time.deltaTime;

            // Time to run these are around 20x a second
            if (m_timer > .05)
            {
                m_timer = 0;
                // ============ Lists Closest Players ============
                foreach (Player player in GameManager.GetPlayers())
                {
                    if (player == null) continue;

                    float dist = Vector3.Distance(m_ball.transform.position, player.transform.position);
                    m_playerDistances[player.NetworkObjectId] = dist;

                    if (player.playerCollider.bounds.Intersects(m_collider.bounds))
                    {
                        TouchedBall?.Invoke(player, dist);
                        PlayerLastTouched = player.NetworkObjectId;
                    }
                }
                // ============ Sorts Closest Players ============
                m_pairs = from pair in m_playerDistances orderby pair.Value descending select pair;
            }
        }
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {
        if (IsServer)
        {
            if (moved)
            {
                breakShot = HandleSphereCast(pos, maxDistance);
                moved = false;
            }

            // ============ Updates BallStates ============
            // ============ Loose Ball ============
            if (State == BallState.LOOSE)
            {
                m_body.isKinematic = false;
                if (m_pairs == null) return;

                foreach (KeyValuePair<ulong, float> pair in m_pairs)
                {
                    if (pair.Value < 1.5f && !m_insideHitboxShot)
                    {
                        Debug.Log(pair.Key + " picked up ball");
                        // Player closest to ball picks up the ball.
                        State = BallState.HELD;
                        ChangeBallHandler(pair.Key);
                        break;
                    }
                }
            }
            // ============ Ball Held ============
            else if (State == BallState.HELD)
            {
                m_body.isKinematic = true;

                // Tells the ball which hand to be in.
                // These should be ok to not be in FixedUpdate
                if (m_currentPlayer.isBallInLeftHand)
                    m_body.MovePosition(m_currentPlayer.GetLeftHand().transform.position);
                else
                    m_body.MovePosition(m_currentPlayer.GetRightHand().transform.position);
            }
            // ============ Ball Shoot ============
            else if (State == BallState.SHOT)
            {
                ChangeBallHandler(NO_PLAYER);
            }
        }
    }

    public void OnGameStarted()
    {
        if (IsServer)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            StopBall();
            m_body.MovePosition(new Vector3(5, 3, 5));
            ChangeBallHandler(NO_PLAYER);
        }
    }

    public void OnChangeBallHandler(ulong lastPlayer, ulong newPlayer)
    {
        if (shotInAction)
        {
            OnShotMissed();
        }
    }

    // =================================== RPCs ===================================
    public void OnShoot(ulong netID, ShotData shotData, ShotBarData shotBarData)
    {
        m_shooter = netID;
        m_shotData = shotData;
        m_shotBarData = shotBarData;
        m_body.isKinematic = false;
        Reset();
    }

    [ServerRpc]
    public void AnimationReleaseServerRpc()
    {
    }

    // =================================== Public Functions ===================================
    public void StopBall()
    {
        m_body.velocity = Vector3.zero;
    }

    public void BallFollowArc(ulong netID, float releaseDist, float releaseDiff)
    {
        if (!IsServer) return;

        // Dunks are handle in PlayerUtils.Dunk()
        // Maybe I should move that to Player but I dont think
        // it should be in here because the everything is done to
        // the player and the Ball is just following the hand position.
        if (m_shotData.type == ShotType.DUNK) return;

        Player player = GameManager.GetPlayerByNetworkID(netID);

        State = BallState.SHOT;
        shotInAction = true;

        float h = ShotController.GetShotRange(m_shotData.type)
            == ShotRange.LONG ? UnityEngine.Random.Range(10f, 12f)
            : UnityEngine.Random.Range(2.5f, 3.5f);
        float d = SHOT_SPEED + UnityEngine.Random.value / m_shotData.distance;

        Vector3 offset = Vector3.zero;

        // Outdated but kept if changes ever revert
        // Basket is AROUND ~0.8m (circumference) or .8x.8m box
        // Voxel of rim is .2m
        // The ball currently is .6m (circumference) at 1 scale.
        // NBA rim is 18inchs, ball is 9.4inchs (circumference) = ~52% of size
        // The scale of the ball is set to .55
        // This makes the ball size 3.3m
        // .8 - .33 = .47 / 2 = .235m around 0,0,0 of rim
        //
        // Updated ball size to be .9 scale. And rim to be slightly smaller to accomidate the size.
        // This results in around ~.16 meter around the ball if ball is centered at the rim.
        // This is slightly inaccuracte because I have not done the math on the slightly smaller rim
        // Overrall the ball is slighly larger then supposed to at around exactly 50% the size of the rim rather then 52%.

        const float BASE_VAL_INCREMENT = .01f;
        const float HEIGHT_DIFF_MULTIPLIER = 1f;
        const float MAX_HEIGHT_DIFF = 100f;

        float releaseDiffXOffset = Mathf.Clamp(releaseDiff, -MAX_HEIGHT_DIFF, MAX_HEIGHT_DIFF) * HEIGHT_DIFF_MULTIPLIER * BASE_VAL_INCREMENT;

        Vector2 X_GRADE_GOOD = new Vector2(.0f, .05f);
        Vector2 Y_GRADE_GOOD = new Vector2(.0f, .0f);
        Vector2 Z_GRADE_GOOD = new Vector2(.0f, .235f);

        Vector2 X_GRADE_OK = new Vector2(.0f, .1f);
        Vector2 Y_GRADE_OK = new Vector2(.0f, .0f);
        Vector2 Z_GRADE_OK = new Vector2(.1f, .235f * 2f);

        Vector2 X_GRADE_POOR = new Vector2(.1f, .2f);
        Vector2 Y_GRADE_POOR = new Vector2(.1f, .2f);
        Vector2 Z_GRADE_POOR = new Vector2(.1f, .235f * 3f);

        m_grade = m_shotBarData.GetShotGrade(releaseDist);
        m_releaseDiff = releaseDiff;
        m_shotDifficulty = 0;

        if (m_grade == ShotBarData.GRADE_GOOD)
        {
            offset = GetRandOffsetFromRanges(X_GRADE_GOOD, Y_GRADE_GOOD, Z_GRADE_GOOD);
        }
        else if (m_grade == ShotBarData.GRADE_OK)
        {
            offset = GetRandOffsetFromRanges(X_GRADE_OK, Y_GRADE_OK, Z_GRADE_OK);
        }
        else if (m_grade == ShotBarData.GRADE_POOR)
        {
            offset = GetRandOffsetFromRanges(X_GRADE_POOR, Y_GRADE_POOR, Z_GRADE_POOR);
        }

        if (m_grade != ShotBarData.GRADE_PERFECT)
            offset += m_currentPlayer.transform.forward * releaseDiffXOffset;

        if (m_shotData.type == ShotType.LAYUP)
        {
            offset *= 0.33f;
        }
        else if (m_shotData.type == ShotType.POST_MOVE)
        {
            offset *= .50f;
        }
        else if (m_shotData.type == ShotType.SHOT_CLOSE || m_shotData.type == ShotType.POST_SHOT)
        {
            offset *= 0.66f;
        }
        else if (m_shotData.type == ShotType.DUNK)
        {
            offset *= 0f;
        }

        print(offset);
        Vector3 basketPos = GameManager.Singleton.CurrentBasket.netPos.position;

        DebugController.Singleton.PrintConsoleTable("Shot", 128,
            new string[] {"pos", "offset", "grade", "dist", "diff", "target_h",
                "max_h", "perfect", "good", "ok" },
            new object[] { basketPos, offset, m_grade, releaseDist, releaseDiff, ShotMeter.BASE_TARGET_HEIGHT,
                ShotMeter.MAX_TARGET_HEIGHT, m_shotBarData.PerfectLength, m_shotBarData.GoodLength, m_shotBarData.OkLength });


        if (m_shotData.bankshot == BankType.NONE)
            StartCoroutine(FollowArc(m_ball.transform.position, basketPos + offset, h, d));
        else
            StartCoroutine(FollowBackboard(m_shotData, m_ball.transform.position, basketPos + offset, h, d));

    }

    private Vector3 GetRandOffsetFromRanges(Vector2 x, Vector2 y, Vector2 z)
    {
        return new Vector3() {
            x = UnityEngine.Random.Range(x.x, x.y),
            y = UnityEngine.Random.Range(y.x, y.y),
            z = UnityEngine.Random.Range(z.x, z.y)
        };
    }
    private IEnumerator FollowArc(Vector3 start, Vector3 end, float height, float duration)
    {
        float startTime = Time.fixedTime;
        float fracComplete = 0;
        while (fracComplete < 1)
        {
            if (!shotInAction || m_collisionCheck || breakShot) break;
            if (fracComplete < .5f) m_shotPastArc = true;
            // Fraction of journey completed equals current distance divided by total distance.
            fracComplete = (Time.fixedTime - startTime) / duration;
            // Calculate arc
            Vector3 pos = Vector3.Lerp(start, end, fracComplete);
            pos.y += Mathf.Sin(Mathf.PI * fracComplete) * height;

            // Sends out a spherecast to make sure we can move the ball.
            // the rigidbody is move here and returns if we should break the loop.

            this.pos = pos;
            this.maxDistance = fracComplete;
            this.moved = true;
            //if (HandleSphereCast(pos, fracComplete))
                //break;

            yield return new WaitForFixedUpdate();
        }
        breakShot = false;
        m_shotPastArc = false;
        State = BallState.LOOSE;
    }

    // Bank shots and layup
    private IEnumerator FollowBackboard(ShotData shot, Vector3 start, Vector3 end, float height, float duration)
    {
        Vector3 bankPos = GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[(int)shot.bankshot].transform.position;

        float startTime = Time.fixedTime;
        float fracComplete = 0;
        // This is the loop for slerp the ball position from the player to the bank position
        // This statement will break itself at 99% completion of journey
        while (fracComplete < 1)
        {
            if (!shotInAction) break;
            // Fraction of journey completed equals current distance divided by total distance.
            fracComplete = (Time.fixedTime - startTime) / duration;
            // Calculate arc
            Vector3 pos = Vector3.Lerp(start, bankPos, fracComplete);
            pos.y += Mathf.Sin(Mathf.PI * fracComplete) * height;

            // Sends out a spherecast to make sure we can move the ball.
            // the rigidbody is move here and returns if we should break the loop.
            if (HandleSphereCast(pos, fracComplete))
                break;

            yield return new WaitForFixedUpdate();
        }

        //Resets values
        startTime = Time.fixedTime;
        fracComplete = 0;
        // This is the loop for lerp ball position from bank spot on backboard to basket.
        while (fracComplete < 1)
        {
            if (!shotInAction) break;
            // Using duration here does not makes sense since its a constant distance between the bank and the
            // basket. Think about moveing this to a static const or some better way?
            fracComplete = (Time.fixedTime - startTime) / (duration * .75f);

            Vector3 pos = Vector3.Lerp(bankPos, end, fracComplete);
            pos.y += Mathf.Sin(Mathf.PI * fracComplete) * height;

            // Sends out a spherecast to make sure we can move the ball.
            // the rigidbody is move here and returns if we should break the loop.
            if (HandleSphereCast(pos, fracComplete))
                break;

            yield return new WaitForFixedUpdate();
        }

        State = BallState.LOOSE;
    }

    private IEnumerator FingerRoll(ShotData shot, Vector3 start, Vector3 end, float height, float duration)
    {
        yield return null;
    }

    /// Returns the team that does not have possession.
    public int FlipTeam()
    {
        return Mathf.Clamp(Possession ^ 1, 0, 1);
    }

    // =================================== Passing ===================================

    public void TryPassBall(Player passer, int playerSlot, PassType type)
    {
        if (!passer.HasBall) return;
        if (passer.slot == playerSlot) return; //TODO fake pass? here because i have not decides on how to handle this

        Player target = GameManager.GetPlayerBySlot(passer.teamID, playerSlot);
        PassBallServerRpc(passer, target, type);
    }

    public void TryPassBall(ulong passerPid, ulong targetPid, PassType type)
    {

        PassBallServerRpc(passerPid, targetPid, type);
    }

    [ServerRpc]
    public void PassBallServerRpc(ulong passerPid, ulong targetPid, PassType type)
    {
        Player passer = GameManager.GetPlayerByNetworkID(passerPid);
        Player target = GameManager.GetPlayerByNetworkID(targetPid);
        PassBallServerRpc(passer, target, type);
    }


    [ServerRpc]
    public void PassBallServerRpc(Player passer, Player target, PassType type)
    {
        if (!IsServer) return;

        // Set the ball to NO_PLAYER because ball is in air
        ChangeBallHandler(NO_PLAYER);
        // Trigger shot meter for passer
        if (!passer.isAI)
        {
            //passer.InvokeClientRpcOnClient(passer.TriggerRoundShotMeter, passer.OwnerClientId, 1.0f, 1.0f);
            passer.ServerCheckRSM(passer.OwnerClientId, passer.NetworkObjectId, pass_speed, 1.0f, (netID, score) => {
                m_passScore = score;
                print("Pass Score: " + score);
            });
        }
        PassBall?.Invoke(passer, target, m_passScore, type);
        Vector3 endPosition = GetPassPosition(target, 1);
        StartCoroutine(PassDelay(passer, target, endPosition, pass_speed, type));
    }

    [ClientRpc]
    public void PassBallClientRpc(ulong targetPid, Vector3 pos, PassType type, ClientRpcParams cParams = default)
    {
        StartCoroutine(AutoCatchPass(GameManager.GetPlayerByNetworkID(targetPid), pos, type));
    }

    private IEnumerator PassDelay(Player passer, Player target, Vector3 endPosition, float speed, PassType type)
    {
        const float MAX_WAIT = 0.9f;
        float start = Time.time;
        while (true)
        {
            // TODO animations event to trigger this
            if (start + MAX_WAIT < Time.time)
            {
                break;
            }
            yield return null;
        }

        State = BallState.PASS;

        // Tell the client they are getting the balled passed to them.
        PassBallClientRpc(target.NetworkObjectId, endPosition, type, RPCParams.ClientParamsOnlyClient(target.OwnerClientId));

        Debug.Log(string.Format("Pass {0} -> {1} | Type: {2}", passer, target, type));

        // Move the ball by type
        if (type == PassType.CHESS)
            yield return StartCoroutine(Pass(passer, target, endPosition, pass_speed));
        else if (type == PassType.BOUNCE)
            yield return StartCoroutine(BouncePass(passer, target, endPosition, pass_speed));
        else if (type == PassType.LOB)
            yield return StartCoroutine(LobPass(passer, target, endPosition, pass_speed));
    }

    private IEnumerator Pass(Player passer, Player target, Vector3 end, float speed)
    {
        Vector3 start = m_ball.transform.position;
        // Keep a note of the time the movement started.
        float startTime = Time.fixedTime;
        // Calculate the journey length.
        float journeyLength = Vector3.Distance(start, end);
        float fractionOfJourney = 0f;
        while (fractionOfJourney < 1.0f)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.fixedTime - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = distCovered / journeyLength;

            // Sends out a spherecast to make sure we can move the ball.
            // the rigidbody is move here and returns if we should break the loop.
            if (HandleSphereCast(Vector3.Lerp(start, end, fractionOfJourney), distCovered))
                break;

            yield return new WaitForFixedUpdate();
        }
        FinishPass(target.NetworkObjectId);
    }

    public IEnumerator BouncePass(Player passer, Player target, Vector3 end, float speed)
    {
        const float FLOOR_OFFSET = .25f;
        Vector3 start = m_ball.transform.position;
        Vector3 center = (start + end) * 0.5f;
        center.y = FLOOR_OFFSET;

        float startTime = Time.fixedTime;
        float journeyLength = Vector3.Distance(start, center);
        float fractionOfJourney = 0f;
        // Part 1 Ball -> center floor pos
        while (fractionOfJourney < 1.0f)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.fixedTime - startTime) * speed;
            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = distCovered / journeyLength;
            // Set our position as a fraction of the distance between the markers.
            if (HandleSphereCast(Vector3.Lerp(start, end, fractionOfJourney), distCovered))
                break;
            yield return new WaitForFixedUpdate();
        }
        // Reset values;
        startTime = Time.fixedTime;
        journeyLength = Vector3.Distance(center, end);
        fractionOfJourney = 0f;
        // Part 2 center floor pos -> target
        while (fractionOfJourney < 1.0f)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.fixedTime - startTime) * speed;
            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = distCovered / journeyLength;
            // Set our position as a fraction of the distance between the markers.
            if (HandleSphereCast(Vector3.Lerp(start, end, fractionOfJourney), distCovered))
                break;
            yield return new WaitForFixedUpdate();
        }
        FinishPass(target.NetworkObjectId);
    }

    public IEnumerator LobPass(Player passer, Player target, Vector3 end, float speed)
    {
        const float LOB_HEIGHT = 5f;

        Vector3 start = m_ball.transform.position;
        float startTime = Time.fixedTime;
        float fractionOfJourney = 0f;
        while (fractionOfJourney < 1.0f)
        {
            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = (Time.fixedTime - startTime) / speed;
            // Calculate arc
            Vector3 pos = Vector3.Lerp(start, end, fractionOfJourney);
            pos.y += Mathf.Sin(Mathf.PI * fractionOfJourney) * LOB_HEIGHT;
            if (HandleSphereCast(pos, fractionOfJourney))
                break;
            yield return new WaitForFixedUpdate();
        }
        FinishPass(target.NetworkObjectId);
    }

    public IEnumerator AlleyOopPass(Player passer, Player target, Vector3 end, float speed)
    {
        yield return null;
    }

    public IEnumerator FlashyPass(Player passer, Player target, Vector3 end, float speed)
    {
        yield return null;
    }

    /// <summary>
    /// Sends a SphereCast from ball rigidbody to given position.
    /// Will move the rigidbody correctly. If the ray hits a collider
    /// the ball will move towards the contact point. Returns true if a collision
    /// has happen.
    /// </summary>
    /// <param name="pos">Position to move too</param>
    /// <param name="fracComplete">Max move distance</param>
    /// <returns>true if collision too place. false if not</returns>
    private bool HandleSphereCast(Vector3 pos, float fracComplete)
    {
        Physics.SphereCast(m_body.position, .2f, pos, out RaycastHit hitInfo, fracComplete);
        if (!hitInfo.collider)
            m_body.MovePosition(pos);
        else if (hitInfo.collider.isTrigger)
        {
            m_body.MovePosition(pos);
            if (hitInfo.collider.name == "Hitbox Bot")
            {
                hitInfo.collider.SendMessage("OnTriggerEnter", m_collider);
            }
        }
        else if (!hitInfo.collider.isTrigger)
        {
            m_body.MovePosition(hitInfo.point);
            return true;
        }
        return false;
    }


    private void FinishPass(ulong targetNetID)
    {
        ChangeBallHandler(targetNetID);
        State = BallState.HELD;
    }

    private Vector3 GetPassPosition(Player target, int skill)
    {
        Vector3 pos;

        if (target.isSprinting)
            pos = target.CenterPos + target.transform.forward * sprint_offset;
        else if (target.isMoving)
            pos = target.CenterPos + target.transform.forward * walk_offset;
        else
            pos = target.CenterPos + target.transform.forward;

        return pos;
    }

    private IEnumerator AutoCatchPass(Player target, Vector3 pos, PassType type)
    {
        Vector3 start = target.transform.position;
        Vector3 truePos = pos - new Vector3(0, target.height, 0);

        while (State == BallState.PASS)
        {
            float dist = Vector3.Distance(target.CenterPos, pos);
            target.GetMovement().isMovementEnabled = false;
            if (dist > .2)
            {
                target.transform.position = Vector3.Lerp(start, pos, Time.deltaTime * 6.0f);
            }

            yield return null;
        }
        target.GetMovement().isMovementEnabled = true;
        CatchBall?.Invoke(target, type);
    }

    [ServerRpc]
    public void PlayerCallForBallServerRpc(ulong netID)
    {
        Player target = GameManager.GetPlayerByNetworkID(netID);
        if (target == m_currentPlayer) return;
        if (m_currentPlayer.isAI && m_currentPlayer.IsOnOffense() && m_currentPlayer.SameTeam(target))
        {
            PassBallServerRpc(m_currentPlayer, target, PassType.CHESS);
        }
    }

    // =================================== End Passing ===================================

    // =================================== Private Functions ===================================

    private void SetPlayerIDs(ulong netId)
    {
        if (netId != NO_PLAYER)
        {
            PlayerLastPossesion = PlayerWithBall;
            PlayerLastTouched = PlayerWithBall;
        }
        PlayerWithBall = netId;
    }

    const string BACKBOARD_TAG = "Backboard";
    const string RIM_TAG = "RimColliders";
    const string FLOOR_TAG = "Floor";
    private void OnCollisionEnter(Collision collision)
    {
        // backboard is stored in several chunk objects. its
        // parents "Chunks" is tagged.
        if (collision.gameObject.CompareTag(BACKBOARD_TAG))
        {
            m_collisionCheck = true;
        }
        if (collision.gameObject.CompareTag(RIM_TAG))
        {
            m_collisionCheck = true;
            m_hitRim = true;
        }
        if (collision.gameObject.CompareTag(FLOOR_TAG))
        {
            if (shotInAction)
                OnShotMissed();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Ball Rigidbody -> AboveRim Collider
        if (collision.gameObject.CompareTag("AboveRim"))
        {
            m_isAboveRim = true;
            // Ball Collider -> Player Rigidboy
            if (collision.gameObject.CompareTag("Player"))
            {
                // If both then goaltend
                print("goaltend");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("AboveRim"))
        {
            m_isAboveRim = false;
        }
    }

    public void OnShotMade(int teamID)
    {
        if (IsServer && shotInAction)
        {
            GameManager.Singleton.AddScore(teamID, m_shotData.shotValue);
            ShotResultData result = GetShotResultData(ShotResultType.MADE);
            DebugController.Singleton.PrintObjAsTable(result);
            ShotMade?.Invoke(teamID, m_shotData, result);
            Reset();
            print("made");
        }
    }

    public void OnShotMissed()
    {
        if (IsServer)
        {
            ShotResultData result = GetShotResultData(ShotResultType.MISSED);
            ShotMissed?.Invoke(m_shotData, result);
            Reset();
            print("missed");
        }
    }

    private ShotResultData GetShotResultData(ShotResultType type)
    {
        return new ShotResultData() {
            shotMissedType = type,
            grade = m_grade,
            releaseDiff = m_releaseDiff,
            shotDifficulty = m_shotDifficulty
        };
    }

    [ClientRpc]
    public void SetPlayerHandlerClientRpc(ulong netID, bool isHandler, ClientRpcParams cParams = default)
    {
        Player passer = GameManager.GetPlayerByNetworkID(netID);
        passer.isDribbling = isHandler;

        if (isHandler)
        {
            m_currentPlayer.isBallInLeftHand = !m_currentPlayer.isRightHanded;
        }
    }

    /// <summary>
    /// Changes the Player with ball and updates clients that have the ball.<br></br>
    /// If NO_PLAYER id is set sets State to LOOSE.<br></br>
    /// Possession is also changed to correct possession if needed.
    /// </summary>
    private void ChangeBallHandler(ulong newNetworkID)
    {
        //if (newNetworkID == PlayerWithBall) return;

        SetPlayerIDs(newNetworkID);

        if (newNetworkID == DUMMY_PLAYER) return;

        // Alerts the last player with possession if not null hes not holding the ball.
        if (m_currentPlayer != null)
            SetPlayerHandlerClientRpc(m_currentPlayer.NetworkObjectId, false, RPCParams.ClientParamsOnlyClient(m_currentPlayer.OwnerClientId));

        int teamToSwitch = (int)TeamType.NONE;
        // If newPlayer is set to NO_PLAYER id the ball should be loose.
        // There is no need to update that client or change possession until ball is picked up.
        if (newNetworkID == NO_PLAYER)
        {
            if (State != BallState.SHOT)
                State = BallState.LOOSE;
            return;
        }
        else
        {
            m_currentPlayer = GameManager.GetPlayerByNetworkID(newNetworkID);
            SetPlayerHandlerClientRpc(m_currentPlayer.NetworkObjectId, true, RPCParams.ClientParamsOnlyClient(m_currentPlayer.OwnerClientId));
            BallHandlerChange?.Invoke(PlayerLastPossesion, PlayerWithBall);

            // if old id and new id are same don't change teams.
            if (Possession == m_currentPlayer.teamID) return;

            // Temp like this in case i want to change something.
            if (IsBallLoose())
                teamToSwitch = m_currentPlayer.teamID;
            else
                teamToSwitch = m_currentPlayer.teamID;
        }
        ChangePossession(teamToSwitch, false, false);
    }

    private void SetupInbound(int team, GameObject inbound)
    {
        // Fade Screen

        // Move players/ball

        // Setup Inbound Coroutine
    }

    public void ChangePossession(int team, bool inbound, bool loose)
    {
        GameObject inboundObj = GameManager.Singleton.GetClosestInbound(m_ball.transform.position);

        //ChangeBallHandler(NO_PLAYER);
        print("changing to " + team);
        Possession = team;

        if (inbound)
        {
            State = BallState.INBOUND;

            SetupInbound(team, inboundObj);
        }
        else if (loose)
        {
            State = BallState.LOOSE;
        }
        PossessionChange?.Invoke(team);
    }

    public bool IsBallLoose()
    {
        return Possession == -1;
    }

    public bool IsBallNotInPlay()
    {
        return Possession < -1 && Possession > 1;
    }

    private int RandNegOrPos()
    {
        return (UnityEngine.Random.value > .5f) ? 1 : -1;
    }

    private void Reset()
    {
        shotInAction = false;
        hitTopTrigger = false;
        m_collisionCheck = false;
    }

}
