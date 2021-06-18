using MLAPI;
using System.Collections;
using UnityEngine;

public class ShotManager : MonoBehaviour
{

    private const float BASE_SPEED = 50.0f;

    // =================================== Private Varibles ===================================

    private ShotData m_shotData;
    private ShotBarData m_shotBarData;
    private float m_releaseDist;
    private float m_releaseDiff;
    private float m_rttOffset;

    // =================================== MonoBehaviour Functions ===================================

    private void Awake()
    {
        m_shotData = new ShotData();
        m_shotBarData = new ShotBarData();
    }

    // =================================== Public Functions ===================================

    /***
     * Starting a shot starts here.
     */
    public void OnShoot(ulong netID, Player p, float rttDelay)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        float dist = Vector3.Distance(p.transform.position, p.TargetPos);
        float angle = Quaternion.Angle(transform.rotation, p.TargetRotation);

        m_shotData.shooter = netID;
        m_shotData.position = p.transform.position;
        m_shotData.distance = dist;
        m_shotData.direction = GetShotDirection(angle);
        m_shotData.type = ShotController.GetShotType(p, dist, m_shotData.direction);
        m_shotData.style = ShotController.GetShotStyle(p, dist, m_shotData.direction, m_shotData.type);
        m_shotData.bankshot = IsBankShot(p, m_shotData.type);
        m_shotData.leftHanded = p.props.isBallInLeftHand;
        m_shotData.contest = p.GetContestRating();
        m_shotData.shotValue = GetShotValue(p);
        m_shotData.offSkill = 50.0f;
        m_shotData.defSkill = 50.0f;
        m_shotData.passRating = 50.0f;

        m_shotBarData.speed = 35f;
        m_shotBarData.targetOffset = 0f;
        m_shotBarData.targetFadeSpd = 0f;
        m_shotBarData.barShake = 0f;
        m_shotBarData.spdVariationID = (int)SpeedVariations.NONE;

        m_shotBarData.SetBarValues(.05f, .15f, .3f);

        // ShotMeter constants are set in ShotMeter script. These have to do with size of ui elements.
        m_shotBarData.targetSize = (ShotMeter.MAX_TARGET_HEIGHT * m_shotBarData.BonusHeight) + ShotMeter.BASE_TARGET;
        m_shotBarData.targetHeight = (ShotMeter.BASE_TARGET_HEIGHT + m_shotBarData.targetOffset);

        GameManager.Instance.ballController.OnShootBegin(netID, m_shotData, m_shotBarData);
         p.ClientShootBallClientRpc(netID, m_shotData, m_shotBarData, p.rpcParams);
        //p.InvokeClientRpcOnEveryone(p.ClientShootBall, m_shotData, m_shotBarData);
        StartCoroutine(ShotQuality(p, rttDelay));
    }

    public void OnRelease(ulong netId, float rttOffset)
    {
        m_rttOffset = rttOffset;
    }

    public ShotData GetShotData() => m_shotData;

    public ShotBarData GetShotBarData() => m_shotBarData;

    // =================================== Private Functions ===================================

    private void HandleShot(ulong netID)
    {
        GameManager.Instance.ballController.CalculateShot(netID, m_releaseDist, m_releaseDiff);
    }

    private int GetShotValue(Player p)
    {
        if (GameManager.Instance.isFreeThrow)
            return 1;
        else if (p.props.isInsideThree)
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
        // rtt delay is how much input lag was on the StartShot
        print("quality " + ShotMeter.MAX_TARGET_HEIGHT + ", rtt = " + rttDelay);
        float timer = rttDelay;
        while (p.props.isShooting)
        {
            timer += m_shotBarData.speed * Time.deltaTime;
            if (timer >= ShotMeter.MAX_TARGET_HEIGHT)
                break;

            yield return null;
        }
        // rtt offset is how much the input lag was on the ReleaseShot
        m_releaseDiff = (m_shotBarData.FinalTargetHeight + rttDelay) - timer;
        m_releaseDist = Mathf.Abs(m_releaseDiff);
        int grade = m_shotBarData.GetShotGrade(m_releaseDist);


        print("Server: " + m_shotBarData.FinalTargetHeight + ", " + timer + ", dist = " + m_releaseDist + ", grade = " + grade + ", diff = " + m_releaseDiff);

        p.ClientReleaseBallClientRpc(m_releaseDist, m_releaseDiff, p.rpcParams);
        HandleShot(p.NetworkObjectId);
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
            if (p.props.movingLeft) return BankType.LEFT;
            else if (p.props.movingRight) return BankType.RIGHT;
            else return GetClosestBank(p.transform.position);
        }
        else if (type == ShotType.SHOT && p.props.isCtrlDown)
        {
            return GetClosestBank(p.transform.position);
        }

        return BankType.NONE;
    }

    private float GetContestRating(Player p)
    {
        float rating = 0;
        foreach (Player badPlayer in Match.matchTeams[p.OtherTeam].slotToPlayer.Values)
        {
            Vector3 badPos = badPlayer.transform.position;
            if (p.m_innerCollider.bounds.Contains(badPos)
                || p.m_outerCollider.bounds.Contains(badPos))
            {
                float dist = Vector3.Distance(badPos, p.transform.position);
                if (badPlayer.props.isContesting)
                    rating += 10;
                else if (badPlayer.props.isBlocking)
                    rating += 5;
            }
        }
        return rating;
    }
    private static Vector3 GetClosestBankPos(Vector3 current)
    {
        float distL = Vector3.Distance(current, GameManager.Instance.baskets[GameManager.Instance.Possession].banks[0].transform.position);
        float distR = Vector3.Distance(current, GameManager.Instance.baskets[GameManager.Instance.Possession].banks[1].transform.position);
        return (distL < distR) ?
            GameManager.Instance.baskets[GameManager.Instance.Possession].banks[0].transform.position :
            GameManager.Instance.baskets[GameManager.Instance.Possession].banks[1].transform.position;
    }

    private static BankType GetClosestBank(Vector3 current)
    {
        float distL = Vector3.Distance(current, GameManager.Instance.baskets[GameManager.Instance.Possession].banks[0].transform.position);
        float distR = Vector3.Distance(current, GameManager.Instance.baskets[GameManager.Instance.Possession].banks[1].transform.position);
        return (distL < distR) ?
            BankType.LEFT :
            BankType.RIGHT;
    }

}
