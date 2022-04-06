using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> ClientPlayerDict { get; private set; } = new Dictionary<ushort, Player>();
    public PlayerSampler PlayerSampler { get; private set; }
    public PlayerUpdater PlayerUpdater { get; private set; }
    public ushort ClientId { get; private set; }

    private void Awake()
    {
        PlayerSampler = GetComponent<PlayerSampler>();
        PlayerUpdater = GetComponent<PlayerUpdater>();
        PlayerUpdater.Player = this;
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

    public static void SpawnPlayer(ushort clientId, Vector3 spawnPosition, bool isLocalPlayer)
    {
        Player player;

        if (isLocalPlayer)
        {
            // spawn prefab for local player
            player = Instantiate(NetworkManager.Instance.LocalPlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<Player>();
        }
        else
        {
            // spawn prefab for remote player
            player = Instantiate(NetworkManager.Instance.RemotePlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<Player>();
        }

        // set player properties
        player.name = $"Player {clientId}";
        player.ClientId = clientId;

        // add player to dict
        ClientPlayerDict.Add(player.ClientId, player);
    }

    /**
     * Messages
     */

    [MessageHandler((ushort)ServerToClientId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        SpawnPlayerData data = new SpawnPlayerData(message);

        bool isLocalPlayer = data.ClientId == NetworkManager.Instance.Client.Id;
        SpawnPlayer(data.ClientId, data.SpawnPosition, isLocalPlayer);

        if (isLocalPlayer)
        {
            NetworkManager.Instance.Tick = data.SynchronizationTick;
            Debug.Log($"Synchronized tick {data.SynchronizationTick} with server");
        }
    }
}