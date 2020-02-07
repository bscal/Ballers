using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkEvent
{
    UNKNOWN,
    GAME_START,
}

public class NetworkEvents : NetworkedBehaviour
{

    public static NetworkEvents Singleton { get; private set; }

    private Dictionary<NetworkEvent, Action> m_eventTable = new Dictionary<NetworkEvent, Action>();

    void Awake()
    {
        Singleton = this;
    }

    public void RegisterEvent(NetworkEvent eName, Action eAction)
    {
        Debug.Assert(eName != 0, "Name is null");
        Debug.Assert(eAction != null, "Event is null");
        m_eventTable.Add(eName, eAction);
    }

    public void UnregisterEvent(NetworkEvent eName)
    {
        m_eventTable.Remove(eName);
    }

    public Action GetEventAction(NetworkEvent eName)
    {
        m_eventTable.TryGetValue(eName, out Action eAction);
        return eAction;
    }

    public void CallEventServer(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeServerRpc(EventServer, eName);
    }

    public void CallEventAllClients(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnEveryone(EventClient, eName);
    }

    public void CallEventOnClient(ulong id, NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnClient(EventClient, id, eName);
    }

    [ServerRPC]
    private void EventServer(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        m_eventTable.TryGetValue(eName, out var e);
        Debug.Assert(e != null, "Event is null");
        if (e != null) e.Invoke();
    }


    [ClientRPC]
    private void EventClient(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        m_eventTable.TryGetValue(eName, out var e);
        Debug.Assert(e != null, "Event is null");
        if (e != null) e.Invoke();
    }

}
