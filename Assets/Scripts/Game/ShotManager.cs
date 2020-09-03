using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

public class ShotManager : MonoBehaviour
{

    public static ShotManager Singleton { get; private set; }

    private ShotManager() { }

    public NetworkedShotData ShotData { get; } = new NetworkedShotData(NetworkConstants.SHOT_CHANNEL, new ShotData());

    // =================================== Private Varibles ===================================

    private bool m_isShot = false;
    private float m_speed;
    private float m_targetHeight;
    private float m_targetBonusHeight;
    private float m_startOffset;
    private float m_endOffset;
    private bool m_leftHanded;
    private BankType m_bankShot;
    private ShotType m_type;
    private ShotDirection m_direction;
    private float m_releaseDist;

    private ShotController m_shotController;

    // =================================== MonoBehaviour Functions ===================================

    void Start()
    {
        Singleton = this;
        m_shotController = GetComponent<ShotController>();
    }

    // =================================== Public Functions ===================================

    /***
     * Starting a shot starts here.
     */
    public void OnShoot(ulong netID, Player p, float speed, float targetHeight, float bonusHeight, float startOffset, float endOffset)
    {
        if (!NetworkingManager.Singleton.IsServer) return;

        float dist = Vector3.Distance(p.transform.position, p.LookTarget);
        float angle = Quaternion.Angle(transform.rotation, p.LookRotation);

        m_isShot = true;
        m_speed = speed;
        m_targetHeight = targetHeight;
        m_targetBonusHeight = bonusHeight;
        m_startOffset = startOffset;
        m_endOffset = endOffset;
        m_leftHanded = p.isBallInLeftHand;
        m_direction = GetShotDirection(angle);
        m_type = m_shotController.GetTypeOfShot(p, dist, m_direction);
        m_bankShot = IsBankShot(p);

        Debug.LogFormat("{0} : {1}", p.transform.position, p.LookTarget);
        Debug.LogFormat("{0} : {1} : {2}", m_type, dist, m_direction);

        ShotData.Value.shooter = netID;
        ShotData.Value.position = p.transform.position;
        ShotData.Value.distance = dist;
        ShotData.Value.direction = m_direction;
        ShotData.Value.type = m_type;
        ShotData.Value.leftHanded = m_leftHanded;
        ShotData.Value.bankshot = m_bankShot;

        p.InvokeClientRpcOnClient(p.ClientShootBall, p.OwnerClientId, m_type, m_leftHanded, speed, bonusHeight, startOffset, endOffset);

        StartCoroutine(ShotQuality(p));
    }

    public void OnRelease(ulong player)
    {
        m_isShot = false;
    }

    // =================================== Private Functions ===================================

    private void HandleShot(ulong netID)
    {
        print("shot: " + m_releaseDist);
        GameManager.GetBallHandling().BallFollowArc(netID);
    }

    /// <summary>
    /// Server handling of shot quality<br></br>
    /// Used delta time and speed increment to determine where player's target should be
    /// </summary>
    private IEnumerator ShotQuality(Player p)
    {
        yield return null;
        float timer = 0.0f;
        float increment = ShotMeter.BASE_SPEED * m_speed;
        while (m_isShot)
        {
            timer += increment * Time.deltaTime;
            yield return null;
        }

        m_releaseDist = Mathf.Abs(m_targetHeight - timer + m_endOffset - m_startOffset);

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

    private BankType IsBankShot(Player p)
    {
        if (m_type == ShotType.LAYUP && ShotController.GetShotRange(m_type) == ShotRange.CLOSE)
        {
            if (p.isDribLeft) return BankType.LEFT;
            else if (p.isDribRight) return BankType.RIGHT;
            else return GetClosestBank(p.transform.position);
        }
        else if (m_type == ShotType.SHOT && p.isCtrlDown)
        {
            return GetClosestBank(p.transform.position);
        }

        return BankType.NONE;
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
