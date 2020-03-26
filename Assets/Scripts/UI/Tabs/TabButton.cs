using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TabManager tabManager;

    //public UnityEvent<TabButton> Select;
    public TabCallback callbacks;
    public Image background;
    public Text text;
    public Selector data;

    public bool noText = false;

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
        data = GetComponent<Selector>();
    }

    public void OnSelect()
    {
        //Select?.Invoke();
        callbacks?.OnSelect(this);
    }

    public void OnDeselect()
    {
        //Deselect?.Invoke();
        callbacks?.OnDeselect(this);
    }
}
