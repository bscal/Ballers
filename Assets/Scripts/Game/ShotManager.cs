using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using UnityEditor.Experimental.GraphView;

public class ShotManager : MonoBehaviour
{

    private const float BASE_SPEED = 50.0f;

    public static ShotManager Singleton { get; private set; }

    private ShotManager() { }

    //public NetworkedShotData ShotData { get; } = new NetworkedShotData(NetworkConstants.SHOT_CHANNEL, new ShotData());

    // =================================== Private Varibles ===================================

    private bool m_isShot = false;
    private ShotData m_shotData;
    private ShotBarData m_shotBarData;
    private float m_releaseDist;
    private float m_rttOffset;

    // =================================== MonoBehaviour Functions ===================================

    private void Awake()
    {
        Singleton = this;
        m_shotData = new ShotData();
        m_shotBarData = new ShotBarData();
    }

    // =================================== Public Functions ===================================

    /***
     * Starting a shot starts here.
     */
    public void OnShoot(ulong netID, Player p, float rttDelay)
    {
        if (!NetworkingManager.Singleton.IsServer) return;

        float dist = Vector3.Distance(p.transform.position, p.LookTarget);
        float angle = Quaternion.Angle(transform.rotation, p.LookRotation);

        m_isShot = true;

        m_shotData.shooter = netID;
        m_shotData.position = p.transform.position;
        m_shotData.distance = dist;
        m_shotData.direction = GetShotDirection(angle);
        m_shotData.type = ShotController.GetShotType(p, dist, m_shotData.direction);
        m_shotData.style = ShotController.GetShotStyle(p, dist, m_shotData.direction, m_shotData.type);
        m_shotData.bankshot = IsBankShot(p, m_shotData.type);
        m_shotData.leftHanded = p.isBallInLeftHand;
        m_shotData.contest = p.GetContestRating();
        m_shotData.shotValue = GetShotValue(p);
        m_shotData.offSkill = 50.0f;
        m_shotData.defSkill = 50.0f;
        m_shotData.passRating = 50.0f;

        m_shotBarData.speed = UnityEngine.Random.Range(2, 2) * BASE_SPEED;
        m_shotBarData.startOffset = 0f;
        m_shotBarData.endOffset = 0f;
        m_shotBarData.targetFadeSpd = 0f;
        m_shotBarData.barShake = 0f;
        m_shotBarData.spdVariationID = (int)SpeedVariations.NONE;

        m_shotBarData.bad = .5f;
        m_shotBarData.ok = .35f;
        m_shotBarData.good = .15f;
        m_shotBarData.perfect = .05f;

        // ShotMeter constants are set in ShotMeter script. These have to do with size of ui elements.
        m_shotBarData.targetSize = (ShotMeter.MAX_TARGET_HEIGHT * m_shotBarData.BonusHeight) + ShotMeter.BASE_TARGET;
        m_shotBarData.targetHeight = (ShotMeter.BASE_TARGET_HEIGHT + m_shotBarData.endOffset);

        GameManager.GetBallHandling().OnShoot(netID, m_shotData, m_shotBarData);
        p.InvokeClientRpcOnClient(p.ClientShootBall, p.OwnerClientId, netID, m_shotData, m_shotBarData);
        //p.InvokeClientRpcOnEveryone(p.ClientShootBall, m_shotData, m_shotBarData);
        StartCoroutine(ShotQuality(p, rttDelay));
    }

    public void OnRelease(ulong netId, float rttOffset)
    {
        m_isShot = false;
        m_rttOffset = rttOffset;
    }

    public ShotData GetShotData() => m_shotData;

    public ShotBarData GetShotBarData() => m_shotBarData;

    // =================================== Private Functions ===================================

    private void HandleShot(ulong netID)
    {
        GameManager.GetBallHandling().BallFollowArc(netID, m_releaseDist);
    }

    private int GetShotValue(Player p)
    {
        if (GameManager.Singleton.isFreeThrow)
            return 1;
        else if (p.isInsideThree)
            return 2;
        else
            return 3;
    }

    /// <summary>
    /// Server handling of shot quality<br></br>
    /// Used delta time and speed increment to determine where player's target should be
    /// </summary>
    private IEnumerator ShotQuality(Player p, float rttDelay)
    {
        yield return null;
        // rtt delay is how much input lag was on the StartShot
        float timer = rttDelay;
        while (m_isShot)
        {
            timer += m_shotBarData.speed * Time.deltaTime;
            yield return null;
        }
        // rtt offset is how much the input lag was on the ReleaseShot
        m_releaseDist = Mathf.Abs(m_shotBarData.FinalTargetHeight - timer - m_rttOffset);
        int grade = m_shotBarData.GetShotGrade(m_releaseDist);
        print("Server: " + m_shotBarData.FinalTargetHeight + ", " + timer + ", dist = " + m_releaseDist + ", grade = " + grade);

        p.InvokeClientRpcOnClient(p.ClientReleaseBall, p.OwnerClientId, m_releaseDist);
        HandleShot(p.NetworkId);
    }

    private ShotDirection GetShotDirection(float angle)
    {
        if (angle > 45)
        {
            if (angle > 125)
            {
                return ShotDirection.BACK;
            }
            else
            {
                return ShotDirection.SIDE;
            }
        }
        else
        {
            return ShotDirection.FRONT;
        }
    }

    private BankType IsBankShot(Player p, ShotType type)
    {
        if (type == ShotType.LAYUP && ShotController.GetShotRange(type) == ShotRange.CLOSE)
        {
            if (p.isDribLeft) return BankType.LEFT;
            else if (p.isDribRight) return BankType.RIGHT;
            else return GetClosestBank(p.transform.position);
        }
        else if (type == ShotType.SHOT && p.isCtrlDown)
        {
            return GetClosestBank(p.transform.position);
        }

        return BankType.NONE;
    }

    private float GetContestRating(Player p)
    {
        float rating = 0;
        foreach (Player badPlayer in GameManager.Singleton.teams[p.OtherTeam].teamSlots.Values)
        {
            Vector3 badPos = badPlayer.transform.position;
            if (p.m_innerCollider.bounds.Contains(badPos)
                || p.m_outerCollider.bounds.Contains(badPos))
            {
                float dist = Vector3.Distance(badPos, p.transform.position);
                if (badPlayer.isContesting)
                    rating += 10;
                else if (badPlayer.isBlocking)
                    rating += 5;
            }
        }
        return rating;
    }
    private static Vector3 GetClosestBankPos(Vector3 current)
    {
        float distL = Vector3.Distance(current, GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[0].transform.position);
        float distR = Vector3.Distance(current, GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[1].transform.position);
        return (distL < distR) ?
            GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[0].transform.position :
            GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[1].transform.position;
    }

    private static BankType GetClosestBank(Vector3 current)
    {
        float distL = Vector3.Distance(current, GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[0].transform.position);
        float distR = Vector3.Distance(current, GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[1].transform.position);
        return (distL < distR) ?
            BankType.LEFT :
            BankType.RIGHT;
    }

}
