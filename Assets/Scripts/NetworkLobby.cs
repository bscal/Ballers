﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;

public class NetworkLobby : MonoBehaviour
{

    public bool hostServer = true;
    public string host = "159.89.46.131";
    public int port = 7777;

    // Start is called before the first frame update
    void Start()
    {
        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnDisconnected;

        if (hostServer)
        {
            NetworkingManager.Singleton.OnServerStarted += OnServerReady;
            NetworkingManager.Singleton.StartHost();
            Debug.Log("Starting in server mode.");
        }
        else
        {
            NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectAddress = host;
            NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectPort = port;
            NetworkingManager.Singleton.StartClient();
            Debug.Log("Starting in client mode.");
        }
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

    void OnConnected(ulong client)
    {
        Debug.Log("Client Connected: " + client);
        Debug.Log("Connected to " + NetworkingManager.Singleton.ConnectedHostname);
    }

    void OnDisconnected(ulong client)
    {
        Debug.Log("Client Disconnected: " + client);
    }

    void OnServerReady()
    {
        Debug.Log("Server Started");
    }
}
