using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchLoader : NetworkedBehaviour
{

    public GameObject playerPrefab;

    [ServerRPC]
    public void PlayerLoaded(ulong pid)
    {
        GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkedObject no = go.GetComponent<NetworkedObject>();
        no.SpawnAsPlayerObject(pid, null, false);

        InvokeClientRpcOnClient(PlayerLoaded, pid);
    }

    [ClientRPC]
    public void PlayerLoaded()
    {
        GameManager.Singleton.LocalPlayerLoaded();
    }

    [ClientRPC]
    public void AllPlayersLoaded()
    {

    }

    public void Load()
    {
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        yield return null;
        InvokeServerRpc(PlayerLoaded, NetworkingManager.Singleton.LocalClientId);
    }

}
