using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

public class ShotManager : MonoBehaviour
{

    private static readonly NetworkedVarSettings settings = new NetworkedVarSettings() {
        SendChannel = "GameChannel",
        ReadPermission = NetworkedVarPermission.Everyone,
        SendTickrate = 0,
        WritePermission = NetworkedVarPermission.ServerOnly,
    };

    public NetworkedShotData ShotData { get; private set; }

    // =================================== Private Varibles ===================================

    private bool m_isShot = false;
    private float m_speed;
    private float m_targetHeight;
    private float m_targetBonusHeight;
    private float m_startOffset;
    private float m_endOffset;
    private ShotType m_type;
    private float m_releaseDist;

    // =================================== MonoBehaviour Functions ===================================

    void Start()
    {
        ShotData = new NetworkedShotData(settings, new ShotData());

        if (NetworkingManager.Singleton.IsServer)
        {

        }
    }

    // =================================== Public Functions ===================================

    public void OnShoot(ulong player, ShotType type, float speed, float targetHeight, float bonusHeight, float startOffset, float endOffset)
    {
        if (!NetworkingManager.Singleton.IsServer) return;

        Player p = GameManager.GetPlayer(player);

        m_isShot = true;
        m_speed = speed;
        m_targetHeight = targetHeight;
        m_targetBonusHeight = bonusHeight;
        m_startOffset = startOffset;
        m_endOffset = endOffset;
        m_type = type;

        float dist = Vector3.Distance(p.transform.position, p.LookTarget);
        float angle = Quaternion.Angle(transform.rotation, p.LookRotation);

        ShotData.Value.shooter = player;
        ShotData.Value.position = p.transform.position;
        ShotData.Value.distance = dist;
        ShotData.Value.direction = GetShotDirection(angle);
        ShotData.Value.type = m_type;

        StartCoroutine(ShotQuality(p, player));
    }

    public void OnRelease(ulong player)
    {
        m_isShot = false;
    }

    // =================================== Private Functions ===================================

    private void HandleShot(ulong player)
    {
        print("shot: " + m_releaseDist);
    }

    /// <summary>Server handling of shot quality<br></br>
    /// Used delta time and speed increment to determine where player's target should be</summary>
    private IEnumerator ShotQuality(Player p, ulong player)
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

        p.InvokeClientRpcOnClient(p.ClientReleaseBall, player, m_releaseDist);
        HandleShot(player);
    }

    private ShotDirection GetShotDirection(float angle)
    {
        if (angle > 45)
        {
            if (angle > 125)
            {
                print("fade");
                return ShotDirection.BACK;
            }
            else
            {
                print("side");
                return ShotDirection.SIDE;
            }
        }
        else
        {
            print("front");
            return ShotDirection.FRONT;
        }
    }

}
