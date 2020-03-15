using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class ShotManager : MonoBehaviour
{
    private const float MAX_DISTANCE = 30.0f;

    private bool m_isShot = false;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer)
        {

        }
    }

    public void OnShoot(ulong player, float speed, float targetHeight, float startOffset, float endOffset)
    {
        m_isShot = true;
        StartCoroutine(ShotQuality(player, speed, targetHeight, startOffset, endOffset));
    }

    public void OnRelease(ulong player)
    {
        m_isShot = false;
    }

    private void HandleShot(ulong player,float speed, float targetHeight, float meterOffset)
    {
        Player p = GameManager.GetPlayer(player);

        float d = Vector3.Distance(p.transform.position, p.LookTarget);

        print("shot: " + meterOffset);
        print("dist: " + d);

        // TODO handle shots chances
        // TODO if made handle inbound
        // TODO if miss handle rebound physics
    }

    private IEnumerator ShotQuality(ulong player, float speed, float targetHeight, float startOffset, float endOffset)
    {
        yield return null;
        float timer = 0.0f;
        float increment = ShotMeter.BASE_SPEED * speed;
        while (m_isShot)
        {
            timer += increment * Time.deltaTime;
            yield return null;
        }

        float dist = Mathf.Abs(targetHeight - timer + endOffset - startOffset);
        HandleShot(player, speed, targetHeight, dist);
    }

    public void HandleRebound()
    {

    }

}
