using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;

public class NetworkLobby : MonoBehaviour
{

    public bool hostServer = true;

    // Start is called before the first frame update
    void Start()
    {
        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost();
        Debug.Log("Starting in server mode.");
    }

    void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Approving...");
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;

        ulong? prefabHash = SpawnManager.GetPrefabHashFromGenerator("MyPrefabHashGenerator"); // The prefab hash. Use null to use the default player prefab

        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, prefabHash, approve, Vector3.zero, Quaternion.identity);
    }
}
