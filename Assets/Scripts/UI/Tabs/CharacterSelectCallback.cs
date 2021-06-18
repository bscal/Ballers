using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectCallback : TabCallback
{
    public Camera profileCam;

    private CharacterUI m_charUi;
    private ClientPlayer m_client;

    private void Start()
    {
        m_charUi = GameObject.Find("CharacterUI").GetComponent<CharacterUI>();
        m_client = ClientPlayer.Instance;
    }

    public override void OnDeselect(TabButton tabButton) {}

    public override void OnSelect(TabButton tabButton)
    {
        CharacterData cData = m_client.CharData;

        m_charUi.charName.text = CharacterUI.FormatName(cData.firstname, cData.lastname);

        m_charUi.position.text = CharacterUI.FormatPos(cData.position);
        m_charUi.height.text = CharacterUI.FormatHeight(cData.height);
        m_charUi.weight.text = CharacterUI.FormatWeight(cData.weight);
        m_charUi.wingspan.text = CharacterUI.FormatHeight(cData.wingspan);

        // Uses reflection to set UI values for stats
        Type to = m_charUi.skillTexts.GetType();
        Type from = cData.stats.GetType();
        for (int i = 0; i < to.GetFields().Length; i++)
        {
            FieldInfo fTo = to.GetFields()[i];
            FieldInfo fFrom = from.GetFields()[i];
            if (fTo == null || fFrom == null) continue;
            Text t = (Text)fTo.GetValue(m_charUi.skillTexts);
            t.text = fFrom.GetValue(cData.stats).ToString();
        }

        if (profileCam)
            profileCam.gameObject.SetActive(true);
    }

    //private string GetStat(CharacterData cData, string key)
    //{
    //    cData.stats.TryGetValue(key, out int val);
    //    return val.ToString();
    //}
}
