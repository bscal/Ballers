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
    }

    [ClientRPC]
    public void PlayersLoaded()
    {

    }
}
