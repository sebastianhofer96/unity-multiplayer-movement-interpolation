using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> ClientPlayerDict { get; private set; } = new Dictionary<ushort, Player>();
    public ushort ClientId { get; private set; }
    public PlayerController PlayerController => m_playerController;

    private static readonly Vector3 SPAWN_POSITION = new Vector3(0f, 1.03f, -12f);
    private PlayerController m_playerController;

    private void Awake()
    {
        m_playerController = GetComponent<PlayerController>();
    }

    private void OnDestroy()
    {
        // remove self from list when destroyed
        ClientPlayerDict.Remove(ClientId);
    }

    public static Player Get(ushort clientId)
    {
        if (ClientPlayerDict.TryGetValue(clientId, out Player player))
        {
            return player;
        }
        else
        {
            Debug.LogError($"Player for client {clientId} cannot be found");
            return null;
        }
    }

    public static void CreatePlayer(ushort clientId)
    {
        // spawn player on server
        Player player = Instantiate(NetworkManager.Instance.PlayerPrefab, SPAWN_POSITION, Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {clientId}";
        player.ClientId = clientId;
        player.m_playerController.ExpectedTick = NetworkManager.Instance.Tick;

        Debug.Log($"Synchronizing tick {NetworkManager.Instance.Tick} with client {clientId}");

        ClientPlayerDict.Add(player.ClientId, player);

        // spawn player on clients
        NetworkManager.Instance.Server.SendToAll(player.CreateSpawnPlayerMessage());
    }

    public Message CreateSpawnPlayerMessage()
    {
        SpawnPlayerData spawnPlayerData = new SpawnPlayerData
        {
            ClientId = ClientId,
            SpawnPosition = transform.position,
            SynchronizationTick = NetworkManager.Instance.Tick
        };

        return spawnPlayerData.CreateMessage();
    }

    private static void SendAckPlayerIntention(ushort toClientId, long tickToAck)
    {
        AckPlayerIntentionData ackPlayerIntentionData = new AckPlayerIntentionData
        {
            AcknowledgedTick = tickToAck
        };

        NetworkManager.Instance.Server.Send(ackPlayerIntentionData.CreateMessage(), toClientId);
    }

    /**
     * Messages
     */

    [MessageHandler((ushort)ClientToServerId.PlayerIntention)]
    private static void PlayerIntention(ushort fromClientId, Message message)
    {
        PlayerIntentionData data = new PlayerIntentionData(message);

        Player player = Get(fromClientId);
        long tickToAck = player.PlayerController.IntentionHistoryReceived(data);

        // acknowledge tick of newest intention
        if (tickToAck != -1)
        {
            SendAckPlayerIntention(fromClientId, tickToAck);
        }
    }
}