using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientToBackend : MonoBehaviour
{
    void Start()
    {
        print("trying to log in");
        StartCoroutine(Login());
    }

    public IEnumerator Login()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("bscal.me:9090"))
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
            }
        }
    }
}
