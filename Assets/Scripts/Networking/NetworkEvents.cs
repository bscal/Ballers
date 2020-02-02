using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections.Generic;

public class NetworkEvents : NetworkedBehaviour
{

    private Dictionary<string, Func<bool>> m_eventTable;

    public void RegisterEvent(string name, Func<bool> func)
    {
        m_eventTable.Add(name, func);
    }

    public void UnregisterEvent(string name)
    {
        m_eventTable.Remove(name);
    }

    public void CallEventServer(string eventName)
    {
        InvokeServerRpc(EventServer, eventName);
    }

    public void CallEventAllClients(string eventName)
    {
        InvokeClientRpcOnEveryone(EventClient, eventName);
    }

    public void CallEventOnClient(ulong id, string eventName)
    {
        InvokeClientRpcOnClient(EventClient, id, eventName);
    }

    [ServerRPC]
    private void EventServer(string eventName)
    {
        m_eventTable.TryGetValue(eventName, out var e);
        if (e != null) e.Invoke();
    }

    [ClientRPC]
    private void EventClient(string eventName)
    {
        m_eventTable.TryGetValue(eventName, out var e);
        if (e != null) e.Invoke();
    }

}
