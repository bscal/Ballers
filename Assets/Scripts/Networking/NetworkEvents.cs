using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/**
 * Basic Events that are called Registered and Handled
 * With the NetworkEvents class.
 */
public enum NetworkEvent
{
    UNKNOWN,
    GAME_START,
    PLAYER_SHOOT,
    PLAYER_RELEASE
}

/// <summary>
/// Does nothing. Just creating the class to test it out a bit.
/// </summary>
public abstract class NetworkedEventBase : IBitWritable
{
    public readonly int id;
    public event Action<NetworkedEventBase> Performed;

    protected NetworkedEventBase(int id)
    {
        this.id = id;
    }

    public abstract bool Invoke();
    public abstract void Read(Stream stream);
    public abstract void Write(Stream stream);
}

/**
 * Manager class that handles the registration of events and syncing of them to servers
 * and clients
 *
 * Its built on top of MLAPI RPCs. But hopefully this should be more easier and cleaner way of
 * calling smaller events that don't need passing of arguments.
 * 
 * Get its instance from its Singleton property
 *
 * EventHandler are registered in a multi dictionary. Its stored by NetworkEvent -> Dictionary -> Name of object class -> Action
 * You cannot have parameters or return values.
 * If you need these you can use MLAPI base rpcs.
 * You also cannot have multiple different per object handlers. Meaning you cannot have 2 GameManagers that have 2 different functions for an event.
 */

public class NetworkEvents : NetworkedBehaviour
{

    public static NetworkEvents Singleton { get; private set; }

    private Dictionary<NetworkEvent, Dictionary<string, Action>> m_eventTable = new Dictionary<NetworkEvent, Dictionary<string, Action>>();

    void Awake()
    {
        Singleton = this;
    }

    /**
     * Registers a function to an event. Uses the class object name to
     * store multiple events from different objects.
     */
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

    /**
     * Gets an event by NetworkEvent and type name. Returns Action or null if none
     */
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

    /**
     * Calls an events on the server
     */
    public void CallEventServer(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeServerRpc(EventServer, eName);
    }

    /**
     * Calls an event to all clients
     */
    public void CallEventAllClients(NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnEveryone(EventClient, eName);
    }

    /**
     * Calls an event to all clients except id of client given
     */
    public void CallEventAllClientsExcept(ulong id, NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnEveryoneExcept(EventClient, id, eName);
    }

    /**
     * Calls an event to client id given
     */
    public void CallEventOnClient(ulong id, NetworkEvent eName)
    {
        Debug.Assert(eName != 0, "Name is null");
        InvokeClientRpcOnClient(EventClient, id, eName);
    }

    /** MLAPI ServerRPC function for server events.
     *  Checks if specific event is in event table.
     *  If so goes through each registered handler and calls event.
     */

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
            pair.Value?.Invoke();
            print(string.Format("[ NetworkEvent ] Calling Event: {0} from class {1}", eName.ToString(), pair.Key));
        }
    }

    /** MLAPI ClientRPC function for clients events.
    *  Checks if specific event is in event table.
    *  If so goes through each registered handler and calls event.
    */
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
            pair.Value?.Invoke();
            print(string.Format("[ NetworkEvent ] Calling Event: {0} from class {1}", eName.ToString(), pair.Key));
        }
    }

    public void ServerKeyInput(ulong clientID, uint keyID, int type)
    {
        InvokeServerRpc(ServerKeyInputRPC, clientID, keyID, type);
    }

    [ServerRPC]
    private void ServerKeyInputRPC(ulong clientID, uint keyID, int type)
    {
        DebugController.Singleton.PrintConsoleValues("ServerKeyInput", new object[] { clientID, keyID, type }, LogType.INFO);
    }

}
