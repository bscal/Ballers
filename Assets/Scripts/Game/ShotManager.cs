using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class ShotManager : MonoBehaviour
{

    // =================================== Private Varibles ===================================

    private bool m_isShot = false;
    private float m_speed;
    private float m_targetHeight;
    private float m_targetBonusHeight;
    private float m_startOffset;
    private float m_endOffset;

    // =================================== MonoBehaviour Functions ===================================

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer)
        {

        }
    }

    // =================================== Public Functions ===================================

    public void OnShoot(ulong player, float speed, float targetHeight, float bonusHeight, float startOffset, float endOffset)
    {
        if (!NetworkingManager.Singleton.IsServer) return;

        m_isShot = true;
        m_speed = speed;
        m_targetHeight = targetHeight;
        m_targetBonusHeight = bonusHeight;
        m_startOffset = startOffset;
        m_endOffset = endOffset;
        StartCoroutine(ShotQuality(player));
    }

    public void OnRelease(ulong player)
    {
        m_isShot = false;
    }

    // =================================== Private Functions ===================================

    private void HandleShot(ulong player, float releaseDist)
    {
        Player p = GameManager.GetPlayer(player);

        float d = Vector3.Distance(p.transform.position, p.LookTarget);

        print("shot: " + releaseDist);
        print("dist: " + d);

        // TODO handle shots chances
        // TODO if made handle inbound
        // TODO if miss handle rebound physics
    }

    /// <summary>Server handling of shot quality<br></br>
    /// Used delta time and speed increment to determine where player's target should be</summary>
    private IEnumerator ShotQuality(ulong player)
    {
        yield return null;
        float timer = 0.0f;
        float increment = ShotMeter.BASE_SPEED * m_speed;
        while (m_isShot)
        {
            timer += increment * Time.deltaTime;
            yield return null;
        }

        HandleShot(player, Mathf.Abs(m_targetHeight - timer + m_endOffset - m_startOffset));
    }

}
