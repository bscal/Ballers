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
    void Start()
    {

        print("trying to log in...");
        StartCoroutine(Login(SteamUser.GetSteamID().m_SteamID));

        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            print(name);

            print(SteamUser.GetSteamID());
        }
    }

    public IEnumerator Login(ulong steamid)
    {
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
                print(data);
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

    public override bool Equals(object obj)
    {
        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return steamid.GetHashCode();
    }
}
