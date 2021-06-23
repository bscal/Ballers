using UnityEngine;

public class UICallbacks : MonoBehaviour
{

    private Player m_player;
    private ClientNetworkHandler m_networkHandler;

    void Start()
    {
        ClientPlayer.Instance.Initilized += OnClientInitilized;
    }

    private void OnClientInitilized(Player player, ClientNetworkHandler networkHandler)
    {
        m_player = player;
        m_networkHandler = networkHandler;
    }

    public void PlayerReadyUp()
    {
        if (m_networkHandler)
            m_networkHandler.SetReadyStatus();
    }
}
