using System.Collections.Generic;
using UnityEngine;

public class KeyboardMouseInput : IPlayerInputSource
{
    private List<PlayerIntention> m_recordedSamples = new List<PlayerIntention>();
    private bool m_isRecording = true;
    private MainUIManager m_uiManager;

    public KeyboardMouseInput(MainUIManager uiManager)
    {
        m_uiManager = uiManager;
        m_uiManager.SetPlayerInputText("press 1 to save player input");
    }

    public PlayerIntention SampleIntention(long tick)
    {
        PlayerIntention playerInput = new PlayerIntention
        {
            Tick = tick,
            MoveForward = Input.GetKey(KeyCode.W),
            MoveBackward = Input.GetKey(KeyCode.S),
            MoveLeft = Input.GetKey(KeyCode.A),
            MoveRight = Input.GetKey(KeyCode.D),
            LookHorizontal = Input.GetAxis("Mouse X"),
            LookVertical = Input.GetAxis("Mouse Y")
        };

        if (m_isRecording)
        {
            m_recordedSamples.Add(playerInput);
        }

        return playerInput;
    }

    public void SavePlayerInput()
    {
        if (!m_isRecording) return;

        Utils.ExportAsCSV(m_recordedSamples, Utils.INTENTIONS_FILE);
        m_isRecording = false;

        Debug.Log("Saved player input.");
        m_uiManager.SetPlayerInputText("saved player input as intentions.csv");
    }
}