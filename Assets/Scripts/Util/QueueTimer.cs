
using UnityEngine;
using UnityEngine.UI;

public class QueueTimer : MonoBehaviour
{

    public Text text;
    public bool inQueue = true;

    private float timer = 0.0f;

    void Update()
    {
        if (inQueue)
            timer += Time.deltaTime;

        text.text = string.Format("{0}:{1}", Mathf.Floor(timer / 60), Mathf.RoundToInt(timer % 60));
    }
}
