using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

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

    [SerializeField] private ushort m_port;
    [SerializeField] private ushort m_maxClientCount;
    [SerializeField] private int m_simulationRateHz;
    [SerializeField] private int m_networkRateHz;
    [SerializeField] private int m_packetLossPercentage;
    [SerializeField] private bool m_saveSamples;
    [SerializeField] private GameObject m_playerPrefab;

    private readonly string ARG_PORT = "--port";
    private readonly string ARG_MAX_CLIENT_COUNT = "--max-client-count";
    private readonly string ARG_SIMULATION_RATE = "--simulation-rate";
    private readonly string ARG_NETWORK_RATE = "--network-rate";
    private readonly string ARG_PACKET_LOSS = "--packet-loss";
    private readonly string ARG_SAVE_SAMPLES = "--save-samples";

    private int m_ratioSimulationNetworkRate;
    private float m_packetLossAccumulator = 0.0f;
    private float m_packetLossThreshold;

    public long Tick { get; private set; } = 0;
    public long NextTickToProcess { get; set; } = 0;
    public Server Server { get; private set; }
    public GameObject PlayerPrefab => m_playerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeArguments();
        InitializeApp();

        // setup server
        Server = new Server();
        Server.ClientConnected += NewPlayerConnected;
        Server.ClientDisconnected += PlayerLeft;

        // start server
        Server.Start(m_port, m_maxClientCount);
    }

    private void FixedUpdate()
    {
        // handle received messages since last tick
        Server.Tick();

        // nothing to be done when no players connected
        if (Server.ClientCount == 0) return;

        // update game state for current tick
        GameStateManager.UpdateState(Tick);

        // send player updates to clients at defined rate
        if (Tick % m_ratioSimulationNetworkRate == 0)
        {
            // simulate packet loss
            if (m_packetLossPercentage == 0 || m_packetLossAccumulator < m_packetLossThreshold)
            {
                PlayerController.SendPlayerUpdates();
            }
            else
            {
                m_packetLossAccumulator -= m_packetLossThreshold;
                Debug.LogWarning($"Suppressed sending game state update {GameStateManager.CurrentStateTick} due to packet loss configuration");
            }

            if (m_packetLossPercentage != 0) m_packetLossAccumulator++;
        }

        // advance server tick
        Tick++;
    }

    private void InitializeApp()
    {
        if (m_saveSamples)
        {
            List<PlayerIntention> playerIntentions = Utils.ImportFromCSV<PlayerIntention>(Utils.INTENTIONS_FILE);
            BenchmarkSampler.Initialize(playerIntentions.Count);
        }

        // network rate needs to be a divisor of simulation rate
        Assert.IsTrue(m_simulationRateHz % m_networkRateHz == 0);
        m_ratioSimulationNetworkRate = m_simulationRateHz / m_networkRateHz;

        // initialize packet loss simulation
        if (m_packetLossPercentage != 0) m_packetLossThreshold = 100.0f / m_packetLossPercentage;

        // set FixedUpdate rate according to server tick rate and match target frame rate
        Time.fixedDeltaTime = 1f / m_simulationRateHz;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = m_simulationRateHz;

        // initialize riptide logger
#if UNITY_EDITOR
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
#else
        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
        RiptideLogger.Initialize(Debug.Log, true);
#endif
    }

    private void InitializeArguments()
    {
        if (Utils.GetArg(ARG_PORT) != null)
            m_port = ushort.Parse(Utils.GetArg(ARG_PORT));

        if (Utils.GetArg(ARG_MAX_CLIENT_COUNT) != null)
            m_maxClientCount = ushort.Parse(Utils.GetArg(ARG_MAX_CLIENT_COUNT));

        if (Utils.GetArg(ARG_SIMULATION_RATE) != null)
            m_simulationRateHz = int.Parse(Utils.GetArg(ARG_SIMULATION_RATE));

        if (Utils.GetArg(ARG_NETWORK_RATE) != null)
            m_networkRateHz = int.Parse(Utils.GetArg(ARG_NETWORK_RATE));

        if (Utils.GetArg(ARG_PACKET_LOSS) != null)
            m_packetLossPercentage = int.Parse(Utils.GetArg(ARG_PACKET_LOSS));

        if (Utils.GetArg(ARG_SAVE_SAMPLES) != null)
            m_saveSamples = bool.Parse(Utils.GetArg(ARG_SAVE_SAMPLES));

        Debug.Log($"port {m_port}, maxClientCount {m_maxClientCount}, simulationRateHz {m_simulationRateHz}, networkRateHz {m_networkRateHz}, packetLossPercentage {m_packetLossPercentage}, saveSamples {m_saveSamples}");
    }

    private void OnApplicationQuit()
    {
        Server.Stop();

        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= PlayerLeft;
    }

    private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
    {
        ushort newClientId = e.Client.Id;

        // create input history for client
        PlayerController.IntentionHistories.Add(newClientId, new List<PlayerIntention>());

        // spawn new player on server and all clients
        Player.CreatePlayer(newClientId);

        // spawn all existing players on new client
        foreach (var player in Player.ClientPlayerDict.Values)
        {
            if (player.ClientId != newClientId)
            {
                Server.Send(player.CreateSpawnPlayerMessage(), newClientId);
            }
        }
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        ushort clientId = e.Id;

        // remove input history of client
        PlayerController.IntentionHistories.Remove(clientId);

        // destroy gameobject of player who left
        Player player = Player.Get(clientId);
        Destroy(player.gameObject);
    }
}