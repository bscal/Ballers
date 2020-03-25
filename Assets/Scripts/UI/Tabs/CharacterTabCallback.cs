using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTabCallback : TabCallback
{

    public GameObject selectorPanel;
    public GameObject selectorTabPrefab;

    private ClientPlayer m_player;
    private Image m_menuPanel;

    public bool Loading = false;

    public override void OnDeselect(TabButton tabButton)
    {
        throw new System.NotImplementedException();
    }

    public override void OnSelect(TabButton tabButton)
    {
        if (Time.time - m_player.lastCharacterUpdate > 1)
        {
            StartCoroutine(LoadCharacters());

        }
    }

    void Start()
    {
        m_player = GameObject.Find("NetworkClient").GetComponent<ClientPlayer>();
        m_menuPanel = GameObject.Find("Menu Panel").GetComponent<Image>();
    }

    private IEnumerator LoadCharacters()
    {
        Loading = true;
        yield return m_player.ReLoadCharacters();

        foreach (CharacterData cData in m_player.characterStats.Values)
        {
            var tab = Instantiate(selectorTabPrefab, selectorPanel.transform);
            var data = tab.GetComponent<Selector>();
            data.botText.text = string.Format("{0} {1}", cData.firstname, cData.lastname);
            data.topText.text = PlayerClassExtensions.GetName(cData.pClass);
            print("made");
        }

        Loading = false;
    }
}
