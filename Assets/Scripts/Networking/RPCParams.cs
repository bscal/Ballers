using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class RPCParams
{

    private static int SIZE = 10;
    private static ulong[] CACHE_ALL = new ulong[SIZE];
    private static ulong[] CACHE_ALL_EXCEPT = new ulong[SIZE - 1];

    private static readonly ulong[] CACHE_ONLY = new ulong[1];

    private static readonly ClientRpcParams CACHE_ONLY_PARAMS = new ClientRpcParams() {
        Send = new ClientRpcSendParams() {
            TargetClientIds = CACHE_ALL
        }
    };

    public static ClientRpcParams ClientParamsOnlyClient(ulong client)
    {
        CACHE_ONLY[0] = client;
        return CACHE_ONLY_PARAMS;
    }

    public static ClientRpcParams ClientParamsAllButClient(ulong client)
    {
        var list = NetworkManager.Singleton.ConnectedClientsList;

        if (ShouldResize(list.Count))
            Resize(list.Count);

        int addIndex = -1;
        for (int i = 0; i < list.Count; i++)
        {
            ulong id = list[i].ClientId;

            if (id == client)
                continue;

            CACHE_ALL_EXCEPT[addIndex++] = id;
        }

        ClientRpcParams cParams = CreateNew();
        cParams.Send.TargetClientIds = CACHE_ALL_EXCEPT;
        return cParams;
    }

    private static bool ShouldResize(int curSize)
    {
        return curSize != SIZE;
    }

    private static void Resize(int size)
    {
        SIZE = size;
        CACHE_ALL = new ulong[SIZE];
        CACHE_ALL_EXCEPT = new ulong[SIZE - 1];
    }

    public static ClientRpcParams CreateNew()
    {
        ClientRpcParams p = new ClientRpcParams {
            Send = new ClientRpcSendParams()
        };
        return p;
    }
}