using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSampler : MonoBehaviour
{
    [SerializeField] private AppSettings m_appSettings;

    public static List<PlayerIntention> PlayerIntentionHistory = new List<PlayerIntention>();

    private readonly float CAMERA_SENSITIVITY = 100f;
    private IPlayerInputSource m_playerInputSource;
    private MainUIManager m_uiManager;

    private void Awake()
    {
        m_uiManager = FindObjectOfType<MainUIManager>();

        // initialize input source
        m_playerInputSource = m_appSettings.UseSimulationInput
            ? (IPlayerInputSource)new SimulationInput(m_uiManager, m_appSettings.SaveSamples)
            : (IPlayerInputSource)new KeyboardMouseInput(m_uiManager);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !m_appSettings.UseSimulationInput)
        {
            ((KeyboardMouseInput)m_playerInputSource).SavePlayerInput();
        }
    }

    public static void SamplePlayerIntention(long tick)
    {
        // get local player
        Player player = Player.Get(NetworkManager.Instance.Client.Id);

        // create player intention
        PlayerIntention playerIntention = player.PlayerSampler.m_playerInputSource.SampleIntention(tick);

        // apply camera sensitivity
        playerIntention.LookHorizontal *= player.PlayerSampler.CAMERA_SENSITIVITY;
        playerIntention.LookVertical *= player.PlayerSampler.CAMERA_SENSITIVITY;

        // add intention to history
        PlayerIntentionHistory.Add(playerIntention);
    }

    public static void SendPlayerIntentionHistory()
    {
        PlayerIntentionData playerIntentionData = new PlayerIntentionData
        {
            BaseTick = PlayerIntentionHistory[0].Tick,
            Entries = new List<PlayerIntentionData.Entry>()
        };

        foreach (var playerIntention in PlayerIntentionHistory)
        {
            playerIntentionData.Entries.Add(playerIntention.CreateEntry(playerIntentionData.BaseTick));
        }

        NetworkManager.Instance.Client.Send(playerIntentionData.CreateMessage());
    }

    /**
     * Messages
     */

    [MessageHandler((ushort)ServerToClientId.AckPlayerIntention)]
    private static void AckPlayerIntention(Message message)
    {
        AckPlayerIntentionData data = new AckPlayerIntentionData(message);

        // check if the acknowledged tick is in the history
        if (data.AcknowledgedTick >= PlayerIntentionHistory[0].Tick)
        {
            // check how many intentions are acknowledged
            int entryCountToRemove = (int)(data.AcknowledgedTick - PlayerIntentionHistory[0].Tick + 1);

            // remove acknowledged intentions from history
            PlayerIntentionHistory.RemoveRange(0, entryCountToRemove);
        }
    }
}