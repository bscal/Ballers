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
    public const string STATUS_OK = "Ok";
    public const string URL = "http://bscal.me";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public static IEnumerator Login(ulong steamid, Action<UserData, string> callback)
    {
        print("Trying to log in...");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(URL + ":9090/login/" + steamid))
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
                callback(userData[0], STATUS_OK);
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

        using (UnityWebRequest www = UnityWebRequest.Post(URL + ":9090/character/create", form))
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

        using (UnityWebRequest www = UnityWebRequest.Post(URL + ":9090/character/delete", form))
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

    public static IEnumerator FetchAIFromServer(int aiPlayerID, Action<CharacterData, string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(URL + ":9090/character/ai/{0}", aiPlayerID)))
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

                CharacterData cData = DataToAICData(JArray.Parse(data));

                callback?.Invoke(cData, STATUS_OK);
            }
        }
    }

    public static IEnumerator SaveCharacter(ClientPlayer cp)
    {
        yield return SaveCharacter(cp.SteamID, cp.Cid, cp.CharData);
    }

    public static IEnumerator SaveCharacter(ulong steamid, int cid, CharacterData cData)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);
        form.AddField("character", JsonConvert.SerializeObject(cData));
        form.AddField("stats", JsonConvert.SerializeObject(cData.stats));

        using (UnityWebRequest www = UnityWebRequest.Post(URL + ":9090/character/save", form))
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
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(URL + ":9090/character/{0}/{1}", steamid, cid)))
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

                CharacterData cData = DataToCData(JArray.Parse(data));

                callback?.Invoke(cData, STATUS_OK);
            }
        }
    }

    public static IEnumerator FetchAllCharacters(ulong steamid, Action<List<CharacterData>, string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(URL + ":9090/character/{0}/all", steamid)))
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
                JToken character = tokens[1];
                JToken characterStats = tokens[2];
                JToken characterSkills = tokens[3];

                List<CharacterData> dataList = new List<CharacterData>(count);
                for (int i = 0; i < count; i++)
                {
                    CharacterData cData = DataToCData(character[i], characterStats[i], characterSkills[i]);
                    dataList.Add(cData);
                }

                callback?.Invoke(dataList, STATUS_OK);
            }
        }
    }

    /// <summary>
    /// Json array of characters info.
    /// </summary>
    private static CharacterData DataToCData(JArray data)
    {
        CharacterData cData = new CharacterData();
        CharacterStats cStats = new CharacterStats();
        CharacterSkills cSkills = new CharacterSkills();

        if (data[0] != null)
            JsonConvert.PopulateObject(data[0].ToString(), cData);
        if (data[1] != null)
            JsonConvert.PopulateObject(data[1].ToString(), cStats);
        if (data[2] != null)
            cSkills.skillMap = StringToDictionary(data[2]["skills_json"]);

        cData.stats = cStats;
        cData.skills = cSkills;
        return cData;
    }

    /// <summary>
    /// Returns deserialized CharaterData
    /// </summary>
    /// <param name="data0">CharaterData json</param>
    /// <param name="data1">CharaterStat json</param>
    /// <param name="data2">CharacterSkill json</param>
    private static CharacterData DataToCData(JToken data0, JToken data1, JToken data2)
    {
        CharacterData cData = new CharacterData();
        CharacterStats cStats = new CharacterStats();
        CharacterSkills cSkills = new CharacterSkills();

        if (data0 != null)
            JsonConvert.PopulateObject(data0.ToString(), cData);
        if (data1 != null)
            JsonConvert.PopulateObject(data1.ToString(), cStats);
        if (data2 != null)
            cSkills.skillMap = StringToDictionary(data2["skills_json"]);

        cData.stats = cStats;
        cData.skills = cSkills;
        return cData;
    }

    /// <summary>
    /// Used for AI character deserialization. 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static CharacterData DataToAICData(JArray data)
    {
        CharacterData cData = new CharacterData();
        CharacterStats cStats = new CharacterStats();
        CharacterSkills cSkills = new CharacterSkills();

        if (data[0] == null) return cData;

        string dataStr = data[0].ToString();
        JsonConvert.PopulateObject(dataStr, cData);
        JsonConvert.PopulateObject(dataStr, cStats);

        // JSON from backend is in an array. It is size 1 for most fetch methods.
        // We get the 1st index and then skills_json object from the json.
        // The string of the object is taken and deserialized into an dictionary to use in
        // cSkills.
        cSkills.skillMap = StringToDictionary(data[0]["skills_json"]);

        cData.stats = cStats;
        cData.skills = cSkills;
        return cData;
    }

    /// <summary>
    /// Returns a deserialized dictionary from a JToken object.
    /// </summary>
    private static Dictionary<string, float> StringToDictionary(JToken token)
    {
        return JsonConvert.DeserializeObject<Dictionary<string, float>>(token.ToString());
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

    public static IEnumerator StartFinding(ulong steamid, int cid, ulong[] partyIds, int[] cidIds)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);
        form.AddField("party_size", partyIds.Length);
        for (int i = 0; i < partyIds.Length; i++)
        {
            form.AddField("memeber_id_" + i, partyIds[i].ToString());
            form.AddField("memeber_cid_" + i, cidIds[i]);
        }

        using (UnityWebRequest www = UnityWebRequest.Post("bscal.me:9090/matchmaking/join", form))
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

    public static IEnumerator StopFinding(ulong steamid)
    {
        using (UnityWebRequest www = UnityWebRequest.Post("bscal.me:9090/matchmaking/leave", steamid.ToString()))
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

    public static IEnumerator UpdateFinding(ulong steamid, int cid, Action<bool, string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format("bscal.me:9090/matchmaking/{0}", steamid)))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Web Error: " + webRequest.error);
                callback?.Invoke(false, webRequest.error);
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                JToken tokens = JArray.Parse(data);
                callback?.Invoke(tokens.ToObject<bool>(), webRequest.error);
            }
        }
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
    [JsonIgnore]
    public CharacterSkills skills;
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

public class CharacterSkills {
    public Dictionary<string, float> skillMap;
}