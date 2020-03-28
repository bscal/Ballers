using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectCallback : TabCallback
{
    public Camera profileCam;

    private CharacterUI m_charUi;
    private ClientPlayer m_client;

    private void Start()
    {
        m_charUi = GameObject.Find("CharacterUI").GetComponent<CharacterUI>();
        m_client = ClientPlayer.Singleton;
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

        m_charUi.Three.text = cData.stats.threeShooting.ToString();


        profileCam.gameObject.SetActive(true);
    }

    //private string GetStat(CharacterData cData, string key)
    //{
    //    cData.stats.TryGetValue(key, out int val);
    //    return val.ToString();
    //}
}
