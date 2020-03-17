using MLAPI;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerManager : NetworkedBehaviour
{
    public static PlayerManager Singleton;

    private const float UPDATE_TIME = 5.0f;

    private Dictionary<Player, CharacterData> m_playerStats;

    private float m_timer;

    // Start is called before the first frame update
    void Start()
    {
        Singleton = this;

        if (IsServer)
        {
            m_playerStats = new Dictionary<Player, CharacterData>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            m_timer += Time.deltaTime;

            if (m_timer > UPDATE_TIME)
            {
                m_timer = 0;
            }
        }
    }

    public float GetPlayerStat(Player player, string stat)
    {
        return 0f;
    }

    public CharacterData GetPlayerStats(Player player)
    {
        return null;
    }

    public float FetchPlayerStat(ulong steamid, int cid, string stat)
    {
        return 0;
    }

    public void FetchPlayerAllStats(ulong steamid, int cid)
    {
        CharacterData cData;
        StartCoroutine(FetchCharacterFromServer(steamid, cid, result => { cData = result; }));
    }

    public CharacterData[] FetchEveryPlayersStats(ulong[] steamids, int[] cids)
    {
        return null;
    }

    public IEnumerator AddCharacter(ulong steamid, int cid)
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
                Debug.Log("Form upload complete!");
            }
        }
    }

    private IEnumerator FetchCharacterFromServer(ulong steamid, int cid, System.Action<CharacterData> callback = null)
    {
        print(string.Format("Fetching player {0} ; cid {1}...", steamid, cid));
        using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format("bscal.me:9090/character/{0}/{1}", steamid, cid)))
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
                var charData = JsonConvert.DeserializeObject<List<CharacterData>>(data);
                print(charData[0].cid);
                if (callback != null) callback.Invoke(charData[0]);
            }
        }
    }

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
}

