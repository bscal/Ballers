using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientToBackend : MonoBehaviour
{

    private PlayerManager m_playerManager;

    void Start()
    {
        m_playerManager = PlayerManager.Singleton;
        
        //m_playerManager.StartCoroutine(m_playerManager.AddCharacter(SteamUser.GetSteamID().m_SteamID, 0));

        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            print(name);
            print(SteamUser.GetSteamID());
        }
    }

    public IEnumerator Load(GameSetup setup)
    {
        setup.clientLoading = true;

        yield return Login(SteamUser.GetSteamID().m_SteamID);

        yield return m_playerManager.FetchCharacterFromServer(SteamUser.GetSteamID().m_SteamID, 0, (result) => {
            print(result);
        });

        setup.clientLoading = false;
    }

    public IEnumerator Login(ulong steamid)
    {
        print("Trying to log in...");
        using (UnityWebRequest webRequest = UnityWebRequest.Get("bscal.me:9090/login/" + steamid))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Web Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Web Received: " + webRequest.downloadHandler.text);

                byte[] results = webRequest.downloadHandler.data;

                string data = webRequest.downloadHandler.text;
                var userData = JsonConvert.DeserializeObject<List<UserData>>(data);
                print(userData[0].created);
            }
        }
    }
}

public struct UserData
{
    [JsonProperty("steamid")]
    public ulong steamid;
    [JsonProperty("date_created")]
    public DateTime created;
    [JsonProperty("last_login")]
    public DateTime lastLogin;
    [JsonProperty("last_char")]
    public int lastChar;
    [JsonProperty("char_index")]
    public int charIndex;

    public override bool Equals(object obj)
    {
        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return steamid.GetHashCode();
    }
}
