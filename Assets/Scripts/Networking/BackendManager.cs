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

    public static IEnumerator SaveCharacter(ulong steamid, int cid, CharacterData cData)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamid.ToString());
        form.AddField("cid", cid);
        form.AddField("json_data", JsonConvert.SerializeObject(cData));

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

                JArray array = JArray.Parse(data);
                foreach (JObject obj in array.Children<JObject>())
                {
                    JsonConvert.PopulateObject(obj.ToString(), cData);
                }

                callback?.Invoke(cData, "Ok");
            }
        }
    }

    public IEnumerator FetchAllCharacters(ulong steamid, Action<List<CharacterData>, string> callback)
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

                JArray array = JArray.Parse(data);
                int count = int.Parse(array[0].ToString());

                List<CharacterData> dataList = new List<CharacterData>(count);
                var objs = array.Children<JObject>();
                for (int i = 1; i < count; i++)
                {
                    dataList.Add(JsonConvert.DeserializeObject<CharacterData>(objs[i].ToString()));
                }

                callback?.Invoke(dataList, "Ok");
            }
        }
    }

    public IEnumerator FetchEveryPlayersStats(ulong[] steamids, int[] cids, float timeout, Action<CharacterData[]> callback)
    {
        CharacterData[] cData = new CharacterData[steamids.Length];
        int callbacksReturned = 0;
        float timer = 0;

        for (int i = 0; i < steamids.Length; i++)
        {
            StartCoroutine(FetchCharacterFromServer(steamids[i], cids[i], (result, err) => {
                if (result == null) return;
                cData[i] = result;
                callbacksReturned++;
            }));
        }

        while (callbacksReturned < steamids.Length)
        {
            yield return new WaitForSeconds(.2f);

            timer += .2f;
            if (timeout > 0 && timeout > timer) break;
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
    [JsonProperty("position")]
    public int position;
    [JsonProperty("height")]
    public int height;
    [JsonProperty("wingspan")]
    public int wingspan;
    [JsonProperty("weight")]
    public int weight;
    [JsonProperty("three_shooting")]
    public int threeShooting;
}
