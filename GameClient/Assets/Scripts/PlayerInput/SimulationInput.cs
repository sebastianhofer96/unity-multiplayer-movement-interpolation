using System.Collections.Generic;
using UnityEngine;

public class SimulationInput : IPlayerInputSource
{
    private List<PlayerIntention> m_playerIntentions;
    private int m_nextIntentionIndex = 0;
    private MainUIManager m_uiManager;

    public SimulationInput(MainUIManager uiManager, bool saveSamples)
    {
        m_playerIntentions = Utils.ImportFromCSV<PlayerIntention>(Utils.INTENTIONS_FILE);

        if (saveSamples)
        {
            BenchmarkSampler.Initialize(m_playerIntentions.Count);
        }

        m_uiManager = uiManager;
        m_uiManager.SetPlayerInputText("simulation input playback");
    }

    public PlayerIntention SampleIntention(long tick)
    {
        PlayerIntention playerIntention = new PlayerIntention();

        if (m_nextIntentionIndex < m_playerIntentions.Count)
        {
            // get next intention
            playerIntention = m_playerIntentions[m_nextIntentionIndex];

            // increase next intention index
            m_nextIntentionIndex++;

            if (m_nextIntentionIndex == m_playerIntentions.Count)
            {
                Debug.Log("Simulation finished.");
                m_uiManager.SetPlayerInputText("simulation finished");
            }
        }

        // set requested tick for intention
        playerIntention.Tick = tick;

        return playerIntention;
    }
}