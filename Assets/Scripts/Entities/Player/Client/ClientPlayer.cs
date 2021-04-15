using MLAPI;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// User, Character, and Player data stored on client side. This is for local client use only
/// </summary>
public class ClientPlayer : MonoBehaviour
{
    public static ClientPlayer Singleton { get; private set; }

    public int Cid { get { return (UserData != null) ? UserData.lastChar : 0; } }
    public UserData UserData { get; private set; }
    public CharacterData CharData
    { 
        get {
            characterStats.TryGetValue(Cid, out CharacterData cData);
            return cData;
        }
        private set
        {
            characterStats.Add(Cid, value);
        }
    }
    public ulong SteamID { get; private set; }

    public ServerPlayerState State { get; set; }

    // This is cached character data. Can be used server or client side.
    // Primarily for non essential or non gameplay tasks. ie. character selection menu
    public Dictionary<int, CharacterData> characterStats = new Dictionary<int, CharacterData>();
    public float lastCharacterUpdate;

    public Player localPlayer;

    //private GameSetup m_gameSetup;
    //private SteamP2PTransport.SteamP2PTransport m_transport;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Singleton = this;
        if (SteamManager.Initialized)
            SteamID = SteamUser.GetSteamID().m_SteamID;
        gameObject.AddComponent(typeof(PlayerHandler));
    }

    void Start()
    {
        //m_gameSetup = GameObject.Find("GameManager").GetComponent<GameSetup>();
        //m_transport = GameObject.Find("NetworkManager").GetComponent<SteamP2PTransport.SteamP2PTransport>();
        //print(m_transport.GetCurrentRtt(OwnerClientId));
        StartCoroutine(Load());
    }

    // Loads locally
    public IEnumerator Load()
    {
        yield return null;

        yield return BackendManager.Login(SteamID, LoginCallback);

        yield return BackendManager.FetchCharacterFromServer(SteamID, Cid, FetchCharacterCallback);

        yield return BackendManager.FetchAllCharacters(SteamID, FetchAllCharacterCallback);

        yield return null;

        print("Finished loading client");
    }

    public IEnumerator ReLoadCharacters()
    {
        yield return BackendManager.FetchAllCharacters(SteamID, FetchAllCharacterCallback);
    }

    // Logins to server
    private void LoginCallback(UserData uData, string err)
    {
        UserData = uData;
    }

    private void FetchCharacterCallback(CharacterData cData, string err)
    {
        CharData = cData;
        UserData.lastChar = cData.cid;
    }

    private void FetchAllCharacterCallback(List<CharacterData> cData, string err)
    {
        foreach (CharacterData c in cData)
        {
            characterStats[c.cid] = c;
        }
        lastCharacterUpdate = Time.time;
    }

    public void ChangeCharacter(int cid)
    {
        StartCoroutine(ChangeCharacterCoroutine(cid));
    }

    private IEnumerator ChangeCharacterCoroutine(int cid)
    {
        yield return BackendManager.SaveCharacter(SteamID, Cid, CharData);
        yield return null;
        yield return BackendManager.FetchCharacterFromServer(SteamID, cid, FetchCharacterCallback);
    }

}
