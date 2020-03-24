using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabCallbacks : MonoBehaviour
{
    public GameObject selectorPanel;
    public GameObject selectorTab;

    private ClientPlayer m_player;
    private Image m_menuPanel;

    public TabButton characterButton;

    public bool Loading = false;

    void Start()
    {
        m_player = GameObject.Find("NetworkClient").GetComponent<ClientPlayer>();
        m_menuPanel = GameObject.Find("Menu Panel").GetComponent<Image>();

        characterButton.Select += TryLoadCharacters;
    }

    public void TryLoadCharacters()
    {
        print(1);
        if (Time.time - m_player.lastCharacterUpdate > 1)
        {
            StartCoroutine(LoadCharacters());
            
        }
    }

    private IEnumerator LoadCharacters()
    {
        Loading = true;
        yield return m_player.ReLoadCharacters();

        foreach (CharacterData cData in m_player.characterStats.Values)
        {
            Instantiate(selectorTab, selectorPanel.transform);
            print("made");
        }

        Loading = false;
    }

}
