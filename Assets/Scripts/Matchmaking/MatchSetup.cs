using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameType
{
    ONES,
    THREES,
    FIVES
}

public class MatchSetup : MonoBehaviour
{

    private const string GAME_SCENE_NAME = "";

    public bool HasStarted { get; private set; }
    public ulong MatchID { get; private set; }
    public CSteamID HostID { get; private set; }

    private Dictionary<byte, ulong> m_playerSlots = new Dictionary<byte, ulong>();
    private GameType m_type;

    void Start()
    {
        
    }

    void Update()
    {

    }

    private void MatchReady()
    {
        HasStarted = true;
    }

    private void SetupServer()
    {

    }

    private IEnumerator ConnectToServer()
    {
        yield return null;
    }
    private void LoadGameScene()
    {

    }

    private void AfterLoadGameScene()
    {

    }

}
