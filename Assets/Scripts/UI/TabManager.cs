using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public Color idleColor;
    public Color enterColor;
    public Color selectedColor;

    public Color textIdleColor;
    public Color textEnterColor;
    public Color textSelectedColor;

    private int m_index;
    private TabButton m_currentButton;
    private List<TabButton> m_tabButtons;

    public List<GameObject> bodies;

    public void Subscribe(TabButton button)
    {
        if (m_tabButtons == null)
        {
            m_tabButtons = new List<TabButton>();
        }

        m_tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (m_currentButton && button == m_currentButton) return;
        button.background.color = enterColor;
        button.text.color = textEnterColor;
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        if (m_currentButton) m_currentButton.OnDeselect();
        m_currentButton = button;
        ResetTabs();

        m_index = button.transform.GetSiblingIndex();
        button.OnSelect();
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].SetActive(i == m_index);
        }

        button.background.color = selectedColor;
        button.text.color = textSelectedColor;
    }

    private void ResetTabs()
    {
        foreach (var button in m_tabButtons)
        {
            if (m_currentButton && button == m_currentButton) continue;
            button.background.color = idleColor;
            button.text.color = textIdleColor;
        }
    }

}
