using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

public class GameStateManager : MonoBehaviour
{
    public static long CurrentStateTick { get; private set; } = -1;

    private static long GetHighestCommonTickAcrossClients()
    {
        // check which ticks are available from all clients and can be processed
        List<long> highestIntentionTicks = new List<long>();

        foreach (var playerIntentions in PlayerController.IntentionHistories.Values)
        {
            if (playerIntentions.Count == 0)
            {
                // no intentions available for client
                highestIntentionTicks.Add(-1);
            }
            else
            {
                // add highest tick from client
                highestIntentionTicks.Add(playerIntentions[playerIntentions.Count - 1].Tick);
            }
        }

        return highestIntentionTicks.Count > 0 ? highestIntentionTicks.Min() : -1;
    }

    private static void ApplyIntentionsForTicks(int tickCountToProcess)
    {
        // process queued ticks
        for (int i = 0; i < tickCountToProcess; i++)
        {
            // iterate clients
            foreach (var (clientId, playerIntentions) in PlayerController.IntentionHistories.Select(d => (d.Key, d.Value)))
            {
                // get next intention for client
                PlayerIntention intention = playerIntentions[0];

                // intention for tick is missing from client due to client not joined yet
                if (intention.Tick > NetworkManager.Instance.NextTickToProcess)
                {
                    Debug.LogWarning($"Client {clientId} never generated intention for tick {NetworkManager.Instance.NextTickToProcess}, client not relevant for this tick");
                    continue;
                }

                Assert.AreEqual(NetworkManager.Instance.NextTickToProcess, intention.Tick, "Ticks are processed out of sync");

                // apply intention of next tick
                Player player = Player.Get(clientId);
                player.PlayerController.Move(intention);
                player.PlayerController.Look(intention);

                // remove intention from history
                playerIntentions.RemoveAt(0);

                // add benchmark samples
                BenchmarkSampler.Add(player.PlayerController.Position, player.PlayerController.Rotation);
            }

            // advance tick to process
            NetworkManager.Instance.NextTickToProcess++;
        }
    }

    public static void UpdateState(long tick)
    {
        long commonTick = GetHighestCommonTickAcrossClients();

        // no intentions from at least one client, abort
        if (commonTick == -1) return;
        
        // only apply intentions with ticks lower or equal to server tick
        if (commonTick > tick) commonTick = tick;

        // process ticks from 'NextTickToProcess' to 'commonTick'
        int tickCount = (int)(commonTick - NetworkManager.Instance.NextTickToProcess + 1);
        ApplyIntentionsForTicks(tickCount);

        // set the tick the game state is currently in
        CurrentStateTick = commonTick;
    }
}