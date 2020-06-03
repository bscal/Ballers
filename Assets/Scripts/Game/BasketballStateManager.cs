using MLAPI;
using MLAPI.NetworkedVar;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BasketballStateManager : NetworkedBehaviour
{

    // Static

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

    // Public Actions

    public event Action OnMatchStarted;
    public event Action OnQuarterEnd;
    public event Action OnHalfEnd;
    public event Action OnMatchEnd;

    // Public

    private NetworkedVarFloat m_inGameTime;
    public float InGameTime { get { return m_inGameTime.Value; } set { m_inGameTime.Value = value; } }

    private NetworkedVarFloat m_shotClock;
    public float ShotClock { get { return m_shotClock.Value; } set { m_shotClock.Value = (byte)value; } }

    public NetworkedVarByte m_state;
    public EMatchState MatchStateValue { get { return (EMatchState)Enum.ToObject(typeof(EMatchState), m_state.Value); } set { m_state.Value = (byte)value; } }
    
    private NetworkedVarByte m_quarter;
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

    private GameManager m_gameManager;
    private byte m_OvertimeCount = 0;
    private bool m_shotclockOff = false;

    private void Start()
    {
        m_UIHomeName.text = "Home";
        m_UIAwayName.text = "Away";
        m_gameManager = GameManager.Singleton;
    }

    public override void NetworkStart()
    {
        m_inGameTime = new NetworkedVarFloat(STATE_SETTINGS);
        m_shotClock = new NetworkedVarFloat(STATE_SETTINGS);
        m_state = new NetworkedVarByte(STATE_SETTINGS);
        m_quarter = new NetworkedVarByte(STATE_SETTINGS);
        if (IsServer)
        {
            //MatchStateValue = EMatchState.PREGAME;
            GameManager.Singleton.GameStarted += OnGameStarted;
        }
    }

    private void Update()
    {
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
            //ShotClock violation
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

    // Private Functions

    public void OnGameStarted()
    {
        InGameTime = QUARTER_LENGTH;
        ShotClock = SHOTCLOCK_LENGTH;
        Quarter = 1;
        MatchStateValue = EMatchState.INPROGRESS;
    }

    private void EndQuarter()
    {
        Quarter++;
        if (Quarter > 2)
        {
            EndHalf();
        }

        else if (Quarter > 4)
        {
            if (Quarter >= byte.MaxValue)
            {
                //End Game
            }

            if (m_gameManager.GetScoreDifference() == 0)
            {
                m_OvertimeCount++;
            }
            // End of regulation
            OnMatchEnd();

        }
        else
        {
            InGameTime = (m_OvertimeCount > 0) ? Mathf.Round(OVERTIME_LENGTH) : Mathf.Round(QUARTER_LENGTH);
            OnQuarterEnd();
        }
    }

    private void EndHalf()
    {
        OnHalfEnd();
    }

    private void UpdateUI()
    {
        m_UIHomeScore.text = m_gameManager.TeamHome.points.ToString();
        m_UIAwayScore.text = m_gameManager.TeamAway.points.ToString();
        m_UIQuarter.text = (m_OvertimeCount > 0) ? "OT" + m_OvertimeCount : Quarter.ToString();
        m_UIClock.text = string.Format("{0}:{1}", Mathf.Floor(InGameTime / 60), Mathf.RoundToInt(InGameTime % 60));
        if (m_shotclockOff)
            m_UIShotClock.text = "";
        else
            m_UIShotClock.text = (ShotClock > 1.0f) ? ShotClock.ToString("F0") : ShotClock.ToString("F1");
    }
}