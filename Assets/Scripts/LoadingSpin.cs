using UnityEngine;
using UnityEngine.UI;

public class LoadingSpin : MonoBehaviour
{

    public Image spinner;


    // Update is called once per frame
    void Update()
    {
        if (spinner == null)
            print("LoadingSpin: Loading Spinner Image not found.");
        else
            spinner.transform.Rotate(0.0f, 0.0f, 1.0f);
    }
}
