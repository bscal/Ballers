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

    public NetworkedVarFloat InGameTime;
    public NetworkedVarFloat ShotClock;
    public NetworkedVarByte MatchStateValue;
    public NetworkedVarByte Quarter;

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
        m_gameManager = GetComponent<GameManager>();
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            InGameTime = new NetworkedVarFloat(STATE_SETTINGS);
            ShotClock = new NetworkedVarFloat(STATE_SETTINGS);
            MatchStateValue = new NetworkedVarByte(STATE_SETTINGS);
            Quarter = new NetworkedVarByte(STATE_SETTINGS);

            MatchStateValue.Value = (byte)EMatchState.PREGAME;
            GameManager.Singleton.OnStartGame += OnStart;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (MatchStateValue.Value == (byte)EMatchState.INPROGRESS)
            {
                IncrementTime(Time.deltaTime);
            }
        }

        UpdateUI();
    }

    internal void IncrementTime(float deltaTime)
    {
        if (MatchStateValue.Value != (byte)EMatchState.INPROGRESS || MatchStateValue.Value != (byte)EMatchState.INBOUND)
            return;

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

    public void SetMatchGameState(EMatchState state)
    {
        MatchStateValue.Value = (byte)state;
    }

    // Private Functions

    public void OnStart()
    {
        InGameTime.Value = QUARTER_LENGTH;
        ShotClock.Value = SHOTCLOCK_LENGTH;
        Quarter.Value = 1;
        MatchStateValue.Value = (byte)EMatchState.INPROGRESS;
    }

    private void EndQuarter()
    {
        Quarter.Value++;
        if (Quarter.Value > 2)
        {
            EndHalf();
        }

        else if (Quarter.Value > 4)
        {
            if (Quarter.Value >= byte.MaxValue)
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
            InGameTime.Value = (m_OvertimeCount > 0) ? Mathf.Round(OVERTIME_LENGTH) : Mathf.Round(QUARTER_LENGTH);
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
        m_UIQuarter.text = (m_OvertimeCount > 0) ? "OT" + m_OvertimeCount : Quarter.Value.ToString();
        m_UIClock.text = string.Format("{0}:{1}", Mathf.Floor(InGameTime.Value / 60), Mathf.RoundToInt(InGameTime.Value % 60));
        if (m_shotclockOff)
            m_UIShotClock.text = "";
        else
            m_UIShotClock.text = (ShotClock.Value > 1.0f) ? ShotClock.Value.ToString("F0") : ShotClock.Value.ToString("F1");
    }
}