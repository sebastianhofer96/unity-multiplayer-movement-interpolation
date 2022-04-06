using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.LogWarning($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private AppSettings m_appSettings;
    [SerializeField] private GameObject m_localPlayerPrefab;
    [SerializeField] private GameObject m_remotePlayerPrefab;
    [SerializeField] private MainUIManager m_uiManager;

    public GameObject LocalPlayerPrefab => m_localPlayerPrefab;
    public GameObject RemotePlayerPrefab => m_remotePlayerPrefab;
    public long Tick { get; set; } = -1;

    public Client Client { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // initialize riptide logging
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        // setup client
        Client = new Client();

        Client.Connected += Connected;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        // handle received messages since last tick
        Client.Tick();

        // abort if tick not yet synchronized with server
        if (Tick == -1) return;

        // sample player intention
        PlayerSampler.SamplePlayerIntention(Tick);
        // update players
        PlayerUpdater.UpdatePlayers();

        // send player intention history
        // network rate needs to be a divisor of simulation rate
        if (Tick % (m_appSettings.SimulationRateHz / m_appSettings.NetworkRateHz) == 0)
        {
            PlayerSampler.SendPlayerIntentionHistory();
        }

        // advance client tick
        Tick++;
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();

        Client.Connected -= Connected;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= PlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }

    public void Connect(string hostAddress)
    {
        Client.Connect(hostAddress);
    }

    private void Connected(object sender, EventArgs e)
    {
        m_uiManager.IsConnected();
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        m_uiManager.ConnectionFailed();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        // destroy gameobject of player who left
        ushort clientId = e.Id;
        Player player = Player.Get(clientId);
        Destroy(player.gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        m_uiManager.ConnectionFailed();

        // reset state
        Tick = -1;
        PlayerSampler.PlayerIntentionHistory.Clear();
        PlayerUpdater.LastPlayerUpdatesReceivedTick = -1;

        // destroy all player gameobjects
        foreach (var player in Player.ClientPlayerDict.Values)
        {
            Destroy(player.gameObject);
        }
    }

    public void DisconnectFromServer()
    {
        if (Client.IsConnected)
        {
            Client.Disconnect();
            DidDisconnect(null, null);
        }
    }
}