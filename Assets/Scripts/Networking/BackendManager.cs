using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class BackendManager : MonoBehaviour
{

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static IEnumerator Login(ulong steamid, Action<UserData, string> callback)
    {
        print("Trying to log in...");
        using (UnityWebRequest webRequest = UnityWebRequest.Get("bscal.me:9090/login/" + steamid))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Web Error: " + webRequest.error);
                callback(null, webRequest.error);
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                Debug.Log("Web Received: " + data);

                var userData = JsonConvert.DeserializeObject<List<UserData>>(data);
                callback(userData[0], "Ok");
            }
        }
    }

    public static IEnumerator AddCharacter(ulong steamid, int cid)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);
        form.AddField("first_name", "player");
        form.AddField("last_name", "name");
        form.AddField("class", 0);
        form.AddField("position", 3);
        form.AddField("height", 6 * 12 + 5);
        form.AddField("wingspan", 6 * 12 + 7);
        form.AddField("weight", 250);

        using (UnityWebRequest www = UnityWebRequest.Post("bscal.me:9090/character/create", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete! Creating character");
            }
        }
    }

    public static IEnumerator DeleteCharacter(ulong steamid, int cid)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);

        using (UnityWebRequest www = UnityWebRequest.Post("bscal.me:9090/character/delete", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete! Deleting Character");
            }
        }
    }

    public static IEnumerator SaveCharacter(ClientPlayer cp)
    {
        yield return SaveCharacter(cp.SteamId, cp.Cid, cp.CharData);
    }

    public static IEnumerator SaveCharacter(ulong steamid, int cid, CharacterData cData)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);
        form.AddField("character", JsonConvert.SerializeObject(cData));
        form.AddField("stats", JsonConvert.SerializeObject(cData.stats));

        using (UnityWebRequest www = UnityWebRequest.Post("bscal.me:9090/character/save", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete! Saving Character");
            }
        }
    }

    public static IEnumerator FetchCharacterFromServer(ulong steamid, int cid, Action<CharacterData, string> callback = null)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format("bscal.me:9090/character/{0}/{1}", steamid, cid)))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Web Error: " + webRequest.error);
                if (callback != null) callback.Invoke(null, webRequest.error);
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                Debug.Log("Web Received: " + data);

                CharacterData cData = new CharacterData();
                CharacterStats sData = new CharacterStats();

                JArray array = JArray.Parse(data);
                JsonConvert.PopulateObject(array[0].ToString(), cData);
                JsonConvert.PopulateObject(array[1].ToString(), sData);

                callback?.Invoke(cData, "Ok");
            }
        }
    }

    public static IEnumerator FetchAllCharacters(ulong steamid, Action<List<CharacterData>, string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format("bscal.me:9090/character/{0}/all", steamid)))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Web Error: " + webRequest.error);
                callback?.Invoke(null, webRequest.error);
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                Debug.Log("Web Received: " + data);

                JToken tokens = JArray.Parse(data);
                int count = int.Parse(tokens[0].ToString());
                JArray character = (JArray)tokens[1];
                JArray characterStats = (JArray)tokens[2];

                List<CharacterData> dataList = new List<CharacterData>(count);
                for (int i = 0; i < count; i++)
                {
                    CharacterData cData = new CharacterData();
                    CharacterStats cStats = new CharacterStats();
                    JsonConvert.PopulateObject(character[i].ToString(), cData);
                    JsonConvert.PopulateObject(characterStats[i].ToString(), cStats);
                    cData.stats = cStats;
                    dataList.Add(cData);
                }

                callback?.Invoke(dataList, "Ok");
            }
        }
    }

    public static IEnumerator FetchEveryPlayers(ulong[] steamids, int[] cids, float timeout, Action<CharacterData[]> callback)
    {
        CharacterData[] cData = new CharacterData[steamids.Length];

        for (int i = 0; i < steamids.Length; i++)
        {
            yield return FetchCharacterFromServer(steamids[i], cids[i], (result, err) => {
                if (result == null) return;
                cData[i] = result;
            });
        }

        callback?.Invoke(cData);
    }
}

public class UserData
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
}

public class CharacterData
{
    [JsonProperty("steamid")]
    public ulong steamid;
    [JsonProperty("cid")]
    public int cid;
    [JsonProperty("first_name")]
    public string firstname;
    [JsonProperty("last_name")]
    public string lastname;
    [JsonProperty("class")]
    public PlayerClass pClass;
    [JsonProperty("position")]
    public int position;
    [JsonProperty("height")]
    public int height;
    [JsonProperty("wingspan")]
    public int wingspan;
    [JsonProperty("weight")]
    public int weight;
    [JsonIgnore]
    public CharacterStats stats;
}

public class CharacterStats
{
    [JsonProperty("close_shooting")]
    public int closeShooting;
    [JsonProperty("mid_shooting")]
    public int midShooting;
    [JsonProperty("three_shooting")]
    public int threeShooting;
    [JsonProperty("free_throw")]
    public int freeThrow;
    [JsonProperty("layup")]
    public int layup;
    [JsonProperty("dunk")]
    public int dunk;
    [JsonProperty("post")]
    public int post;
    [JsonProperty("finishing")]
    public int finishing;
    [JsonProperty("ballhandling")]
    public int ballhandling;
    [JsonProperty("passing")]
    public int passing;
    [JsonProperty("perimeter_d")]
    public int perimeterD;
    [JsonProperty("post_d")]
    public int post_d;
    [JsonProperty("blocking")]
    public int blocking;
    [JsonProperty("speed")]
    public int speed;
    [JsonProperty("strength")]
    public int strength;
    [JsonProperty("vertical")]
    public int vertical;
    [JsonProperty("hands")]
    public int hands;

}
