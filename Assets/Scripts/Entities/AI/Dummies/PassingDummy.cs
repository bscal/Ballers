using System.Collections;
using UnityEngine;

public class PassingDummy : BasicDummy
{
    public IEnumerator ThrowPass(ulong p)
    {
        yield return new WaitForSeconds(3.0f);
        GameManager.Instance.ballController.TryPassBall(m_player, GameManager.GetPlayerByNetworkID(p).props.slot, PassType.CHESS);
    }

}
