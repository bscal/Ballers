using MLAPI;
using MLAPI.NetworkedVar;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameStateManager : NetworkedBehaviour
{

    private static readonly NetworkedVarSettings STATE_SETTINGS = new NetworkedVarSettings()
    {
        SendChannel = "GameState", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 2, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.ServerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    private const float QUARTER_LENGTH          = 60.0f * 6.0f;
    private const float OVERTIME_LENGTH         = QUARTER_LENGTH / 2.0f;
    private const float SHOTCLOCK_LENGTH        = 24.0f;

    public NetworkedVarFloat InGameTime = new NetworkedVarFloat(STATE_SETTINGS);
    public NetworkedVarFloat ShotClock = new NetworkedVarFloat(STATE_SETTINGS);
    public NetworkedVarByte GameStateValue = new NetworkedVarByte(STATE_SETTINGS);
    public NetworkedVarByte MatchGameStateValue = new NetworkedVarByte(STATE_SETTINGS);
    public NetworkedVarByte Quarter = new NetworkedVarByte(STATE_SETTINGS);

    [SerializeField]
    private Text m_UIHomeName;
    [SerializeField]
    private Text m_UIHomeScore;
    [SerializeField]
    private Text m_UIAwayName;
    [SerializeField]
    private Text m_UIAwayScore;
    [SerializeField]
    private Text m_UIQuarter;
    [SerializeField]
    private Text m_UIClock;
    [SerializeField]
    private Text m_UIShotClock;

    private GameManager m_gameManager;
    private bool m_isOvertime = false;
    private bool m_shotclockOff = false;

    private void Start()
    {
        m_UIHomeName.text = "Home";
        m_UIAwayName.text = "Away";
        m_gameManager = GetComponent<GameManager>();
        StartGame();
    }

    private void NetworkedStart()
    {
    }

    private void Update()
    {
        if (MatchGameStateValue.Value == (byte)MatchGameState.INPROGRESS)
        {
            if (IsServer)
                IncrementTime(Time.deltaTime);
        }

        UpdateUI();
    }

    internal void IncrementTime(float deltaTime)
    {
        m_shotclockOff = InGameTime.Value - ShotClock.Value < 0.00f;

        if (!m_shotclockOff && ShotClock.Value < 0.00f)
        {
            //ShotClock violation
        }
        else
        {
            ShotClock.Value -= deltaTime;
        }

        if (InGameTime.Value < 0.00f)
        {
            // End of quarter
            EndQuarter();
        }
        else
        {
            InGameTime.Value -= deltaTime;
        }
    }

    // Public Functions

    public void SetMatchGameState(MatchGameState state)
    {
        MatchGameStateValue.Value = (byte)state;
    }

    public void SetGameState(GameState state)
    {
        GameStateValue.Value = (byte)state;
    }

    // Private Functions

    private void StartGame()
    {
        InGameTime.Value = QUARTER_LENGTH;
        ShotClock.Value = SHOTCLOCK_LENGTH;
        Quarter.Value = 1;
        MatchGameStateValue.Value = (byte)MatchGameState.INPROGRESS;
    }

    private void EndQuarter()
    {
        Quarter.Value++;
        if (Quarter.Value > 4)
        {
            if (Quarter.Value >= byte.MaxValue)
            {
                //End Game
            }

            if (m_gameManager.GetScoreDifference() == 0)
            {
                m_isOvertime = true;
            }
            // End of regulation

        }
        else
        {
            InGameTime.Value = (m_isOvertime) ? Mathf.Round(OVERTIME_LENGTH) : Mathf.Round(QUARTER_LENGTH);
            m_gameManager.EndQuarter();
        }
    }

    private void UpdateUI()
    {
        m_UIHomeScore.text = m_gameManager.TeamHome.points.ToString();
        m_UIAwayScore.text = m_gameManager.TeamAway.points.ToString();
        m_UIQuarter.text = Quarter.Value.ToString();
        m_UIClock.text = string.Format("{0}:{1}", Mathf.Floor(InGameTime.Value / 60), Mathf.RoundToInt(InGameTime.Value % 60));
        if (m_shotclockOff)
            m_UIShotClock.text = "";
        else
            m_UIShotClock.text = (ShotClock.Value > 1.0f) ? ShotClock.Value.ToString("F0") : ShotClock.Value.ToString("F1");
    }
}

// GameState is the overall state of the game.
// Progress of the game loading -> pregrame -> starting
// Not related to the match time.
public enum GameState : byte
{
    NONE,
    PREGAME,
    STARTED,
    PAUSED,
    FINISHING,
    ENDED
}

// The state of the in game match. Is the ball in play, being inbounded, foul called.
public enum MatchGameState : byte
{
    NONE,
    PREGAME,
    INPROGRESS,
    INBOUND,
    FOUL,
    ENDED
}