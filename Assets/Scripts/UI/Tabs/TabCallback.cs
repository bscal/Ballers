using UnityEngine;

public abstract class TabCallback : MonoBehaviour
{
    public abstract void OnSelect(TabButton tabButton);

    public abstract void OnDeselect(TabButton tabButton);

}
