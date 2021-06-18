using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BasketballStateManager : NetworkBehaviour
{

    // Constants

    // FIX currently uses Match.MatchSettings for times
    private const float QUARTER_LENGTH          = 60.0f * 12.0f;
    private const float OVERTIME_LENGTH         = QUARTER_LENGTH / 2.0f;
    private const float SHOTCLOCK_LENGTH        = 24.0f;

    // Public Actions

    public event Action<int, bool> QuarterEnd;
    public event Action HalfEnd;
    public event Action GameEnd;
    public event Action ShotClockViolation;

    // Public
    private NetworkVariableDouble m_inGameTime = new NetworkVariableDouble(NetworkConstants.GAME_STATE_CHANNEL, Match.MatchSettings.QuarterLength);
    public double InGameTime { get { return m_inGameTime.Value; } set { m_inGameTime.Value = value; } }

    private NetworkVariableDouble m_shotClock = new NetworkVariableDouble(NetworkConstants.GAME_STATE_CHANNEL, SHOTCLOCK_LENGTH);
    public double ShotClock { get { return m_shotClock.Value; } set { m_shotClock.Value = value; } }

    public NetworkVariableByte m_state = new NetworkVariableByte(NetworkConstants.GAME_STATE_CHANNEL, (byte)EMatchState.PREGAME);
    public EMatchState MatchStateValue { get { return (EMatchState)Enum.ToObject(typeof(EMatchState), m_state.Value); } set { m_state.Value = (byte)value; } }
    
    private NetworkVariableByte m_quarter = new NetworkVariableByte(NetworkConstants.GAME_STATE_CHANNEL, 1);
    public int Quarter { get { return m_quarter.Value; } set { m_quarter.Value = (byte)value; } }

    // Private

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

    private byte m_OvertimeCount = 0;
    private bool m_shotclockOff = false;

    private void Awake()
    {
        m_UIHomeName.text = "Home";
        m_UIAwayName.text = "Away";
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            GameManager.Instance.GameStartedServer += OnGameStarted;
        }
    }

    private void Update()
    {
        if (!Match.HasGameStarted) return;

        if (IsServer)
        {
            if (MatchStateValue == EMatchState.INPROGRESS)
            {
                IncrementTime(Time.deltaTime);
            }
        }
        
        UpdateUI();
    }

    internal void IncrementTime(float deltaTime)
    {
        if (MatchStateValue != EMatchState.INPROGRESS)
            return;

        m_shotclockOff = (InGameTime - ShotClock) < 0.00f;

        if (!m_shotclockOff && ShotClock < 0.00f)
        {
            ShotClockViolation?.Invoke();
        }
        else
        {
            ShotClock -= deltaTime;
        }

        if (InGameTime < 0.00f)
        {
            // End of quarter
            EndQuarter();
        }
        else
        {
            InGameTime -= deltaTime;
        }
    }

    // Public Functions

    public void SetMatchGameState(EMatchState state)
    {
        MatchStateValue = state;
    }

    public void InitMatchSettings(MatchSettings settings)
    {

    }

    // Private Functions

    private void OnGameStarted()
    {
        MatchStateValue = EMatchState.INPROGRESS;
    }

    private void EndQuarter()
    {
        Quarter++;

        if (Quarter == Match.MatchSettings.QuartersCount / 2)
        {
            EndHalf();
        }

        else if (Quarter > Match.MatchSettings.QuartersCount)
        {
            if (Quarter >= byte.MaxValue)
            {
                //End Game
            }

            if (GameManager.Instance.GetScoreDifference() == 0)
            {
                m_OvertimeCount++;
            }
            // End of regulation
            GameEnd?.Invoke();
        }
        else
        {
            InGameTime = (m_OvertimeCount > 0) ? Mathf.Round(OVERTIME_LENGTH) : Mathf.Round(QUARTER_LENGTH);
            QuarterEnd?.Invoke(Quarter, m_OvertimeCount > 0);
        }
    }

    private void EndHalf()
    {
        HalfEnd?.Invoke();
    }

    private void UpdateUI()
    {
        m_UIHomeScore.text = GameManager.Instance.TeamHome.teamData.points.ToString();
        m_UIAwayScore.text = GameManager.Instance.TeamAway.teamData.points.ToString();
        m_UIQuarter.text = (m_OvertimeCount > 0) ? "OT" + m_OvertimeCount : Quarter.ToString();
        if (InGameTime < 60.0)
            m_UIClock.text = TimeSpan.FromSeconds(InGameTime).ToString("ss\\:f");
        else
            m_UIClock.text = TimeSpan.FromSeconds(InGameTime).ToString("mm\\:ss"); 
        if (m_shotclockOff)
            m_UIShotClock.text = "";
        else
            m_UIShotClock.text = (ShotClock > 1.0) ? ShotClock.ToString("F0") : ShotClock.ToString("F1");
    }
}
