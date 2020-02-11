using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkEvent
{
    UNKNOWN,
    GAME_START,
    PLAYER_SHOOT,
    PLAYER_RELEASE
}

public class NetworkEvents : NetworkedBehaviour
{

    public static NetworkEvents Singleton { get; private set; }

    private Dictionary<NetworkEvent, Dictionary<string, Action>> m_eventTable = new Dictionary<NetworkEvent, Dictionary<string, Action>>();
    //private Dictionary<string, Action> m_eventTable = new Dictionary<string, Action>();

    void Awake()
    {
        Singleton = this;
    }


    public void RegisterEvent(NetworkEvent eName, object eClass, Action eAction)
    {
        Debug.Assert(eName != 0, "Name is null");
        Debug.Assert(eClass != null, "Type is null");
        Debug.Assert(eAction != null, "Event is null");

        if (!m_eventTable.TryGetValue(eName, out var value))
            value = new Dictionary<string, Action>();

        string className = eClass.GetType().Name;

        if (value.ContainsKey(className))
        {
            print("Class already registered under event");
            return;
        }

        value.Add(className, eAction);
        m_eventTable[eName] = value;
        print(string.Format("[ NetworkEvent ] Registered Event {0} from class {1} linked to function {2}", eName.ToString(), eClass.GetType().Name, eAction.Method.Name));

        //m_eventTable.Add(eName.ToString() + eClass.GetType().Name, eAction);
    }

    public void UnregisterEvent(NetworkEvent eName, object eClass)
    {
        if (m_eventTable.TryGetValue(eName, out var value))
            value.Remove(eClass.GetType().Name);

        //m_eventTable.Remove(eName);
    }

    public Action GetEventAction(NetworkEvent eName, object eClass)
    {
        if (m_eventTable.TryGetValue(eName, out var value))
        {
            if (!value.TryGetValue(eClass.GetType().Name, out Action eAction))
                print("eAction is null");
            return eAction;
        }
        return null;

        //m_eventTable.TryGetValue(eName, out Action eAction);
        //return eAction;
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

    public void CallEventAllClientsExcept(ulong id, NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnEveryoneExcept(EventClient, id, eName);
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
        if (!m_eventTable.TryGetValue(eName, out var e))
        {
            Debug.Assert(e != null, "Event is null");
            return;
        }
        foreach (var pair in e)
        {
            if (pair.Value != null) pair.Value.Invoke();
            print(string.Format("[ NetworkEvent ] Calling Event: {0} from class {1}", eName.ToString(), pair.Key));
        }
    }
        


    [ClientRPC]
    private void EventClient(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        if (!m_eventTable.TryGetValue(eName, out var e))
        {
            Debug.Assert(e != null, "Event is null");
            return;
        }
        foreach (var pair in e)
        {
            if (pair.Value != null) pair.Value.Invoke();
            print(string.Format("[ NetworkEvent ] Calling Event: {0} from class {1}", eName.ToString(), pair.Key));
        }
    }

}
