using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TabManager tabManager;

    public event Action Select;
    public event Action Deselect;

    public Image background;
    public Text text;

    public void OnPointerClick(PointerEventData eventData)
    {
        tabManager.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabManager.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabManager.OnTabExit(this);
    }

    void Start()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
        tabManager.Subscribe(this);
    }

    public void OnSelect()
    {
        Select?.Invoke();
    }

    public void OnDeselect()
    {
        Deselect?.Invoke();
    }
}
