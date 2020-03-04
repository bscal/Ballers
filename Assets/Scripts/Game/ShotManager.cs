using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class ShotManager : MonoBehaviour
{

    private bool m_isShot = false;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer)
        {

        }
    }

    public void OnShoot(ulong player, float speed, float height, float startOffset, float endOffset)
    {
        m_isShot = true;
        StartCoroutine(ShotQuality(player, speed, height, startOffset, endOffset));
    }

    public void OnRelease(ulong player)
    {
        m_isShot = false;
    }

    private void HandleShot(ulong player,float speed, float height, float dist)
    {
        print(player);
        print(speed);
        print(height);
        print(dist);

        // TODO handle shots chances
        // TODO if made handle inbound
        // TODO if miss handle rebound physics
    }

    private IEnumerator ShotQuality(ulong player, float speed, float height, float startOffset, float endOffset)
    {
        float timer = 0.0f;
        float increment = ShotMeter.BASE_SPEED * speed;
        while (m_isShot)
        {
            timer += increment * Time.deltaTime;

            yield return null;
        }

        float dist = Mathf.Abs(height - timer + endOffset - startOffset);

        HandleShot(player, speed, height, dist);
    }

    public void HandleRebound()
    {

    }

}
