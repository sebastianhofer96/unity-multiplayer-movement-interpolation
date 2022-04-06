using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerUpdater : MonoBehaviour
{
    [SerializeField] private AppSettings m_appSettings;
    [SerializeField] private Transform m_cameraTransform;

    public Player Player { get; set; }
    public static long LastPlayerUpdatesReceivedTick { get; set; } = -1;

    private IPositionInterpolation m_positionInterpolation;
    private IRotationInterpolation m_rotationInterpolation;
    private IPositionInterpolation m_positionCorrectionInterpolation = new PositionInterpolationLinear();
    private IRotationInterpolation m_rotationCorrectionInterpolation = new RotationInterpolationSlerp();
    private int m_interpolationCount;
    private int m_currentIndex;
    private List<PlayerUpdate> m_updates = new List<PlayerUpdate>();
    private int m_totalUpdateCount = 1; // initial state represents no interpolation
    private int m_ticksElapsed = 0;
    private int m_bufferOffset = 0;
    private Vector2 m_rotation;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector2 Rotation
    {
        get => m_rotation;
        set
        {
            m_rotation = value;
            m_cameraTransform.localRotation = Quaternion.Euler(value.x, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, value.y, 0f);
        }
    }

    private void Awake()
    {
        InitializeInterpolation();
    }

    private void Update()
    {
        // draw look direction
        Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward * 2f, Color.green);
    }

    private void InitializeInterpolation()
    {
        if (!m_appSettings.UseInterpolation) return;

        // init position interpolation
        switch (m_appSettings.PositionAlgorithm)
        {
            case PositionAlgorithm.Linear:
                m_positionInterpolation = new PositionInterpolationLinear();
                break;
            case PositionAlgorithm.CatmullRom:
                m_positionInterpolation = new PositionInterpolationCatmullRom();
                break;
        }

        // init rotation interpolation
        switch (m_appSettings.RotationAlgorithm)
        {
            case RotationAlgorithm.Slerp:
                m_rotationInterpolation = new RotationInterpolationSlerp();
                break;
            case RotationAlgorithm.Squad:
                m_rotationInterpolation = new RotationInterpolationSquad();
                break;
        }

        // init variables
        Assert.AreEqual(m_positionInterpolation.InterpolationCount, m_rotationInterpolation.InterpolationCount, "Both interpolations need to have equal interpolation count");
        m_interpolationCount = m_positionInterpolation.InterpolationCount;
        m_currentIndex = m_interpolationCount / 2;
        m_totalUpdateCount = m_interpolationCount + m_appSettings.BufferSize;
    }

    private bool ThresholdOk(Vector3 newPosition)
    {
        return Vector3.Distance(Position, newPosition) < m_appSettings.ThresholdDistance;
    }

    private void UseTimeInterpolation(List<PlayerUpdate> updatesForPlayer, float timeFraction)
    {
        // interpolate position
        List<Vector3> positions = updatesForPlayer.Select(u => u.Position).ToList();
        Position = m_positionInterpolation.Interpolate(positions, timeFraction);

        // interpolate rotation
        List<Quaternion> rotations = updatesForPlayer.Select(u => Quaternion.Euler(u.Rotation)).ToList();
        Rotation = m_rotationInterpolation.Interpolate(rotations, timeFraction).eulerAngles;
    }

    private void CorrectInterpolation(PlayerUpdate currentUpdate, int globalCurrentIndex, float correction)
    {
        Debug.LogWarning($"Threshold exceeded for player {Player.ClientId}, corrective interpolating to new position");

        // interpolate position
        List<Vector3> positions = new List<Vector3> { Position, currentUpdate.Position };
        Position = m_positionCorrectionInterpolation.Interpolate(positions, correction);

        // interpolate rotation
        List<Quaternion> rotations = new List<Quaternion> { Quaternion.Euler(Rotation), Quaternion.Euler(currentUpdate.Rotation) };
        Rotation = m_rotationCorrectionInterpolation.Interpolate(rotations, correction).eulerAngles;

        // adapt previous updates to interpolate from in the next tick
        m_updates[globalCurrentIndex - 1].Position = Position;
        m_updates[globalCurrentIndex - 1].Rotation = Rotation;
        m_updates[globalCurrentIndex - 1].Tick += m_ticksElapsed;

        // reset ticks elapsed
        m_ticksElapsed = 1;
    }

    private void InterpolateUpdates()
    {
        // determine time fraction and offset for interpolation buffer
        int adjCurrentIndex = m_bufferOffset + m_currentIndex;
        float timeFraction = (float)m_ticksElapsed / (m_updates[adjCurrentIndex].Tick - m_updates[adjCurrentIndex - 1].Tick);

        if (timeFraction > 1.0f && m_bufferOffset < m_appSettings.BufferSize)
        {
            m_bufferOffset++;
            m_ticksElapsed = 1;

            adjCurrentIndex = m_bufferOffset + m_currentIndex;
            timeFraction = (float)m_ticksElapsed / (m_updates[adjCurrentIndex].Tick - m_updates[adjCurrentIndex - 1].Tick);
        }

        // check if interpolation can be applied
        if (timeFraction <= 1.0f)
        {
            if (m_bufferOffset > 0) Debug.Log($"Using interpolation buffer (offset = {m_bufferOffset})");

            // get only relevant updates
            List<PlayerUpdate> updatesForPlayer = m_updates.GetRange(m_bufferOffset, m_interpolationCount);

            // depending on threshold, interpolate updates or apply correction
            if (ThresholdOk(updatesForPlayer[m_currentIndex].Position))
            {
                UseTimeInterpolation(updatesForPlayer, timeFraction);
            }
            else
            {
                CorrectInterpolation(updatesForPlayer[m_currentIndex], adjCurrentIndex, .5f);
            }
        }
        else
        {
            Debug.LogWarning($"Player {Player.ClientId} cannot be updated, need new updates for interpolation");
        }
    }

    private void ApplyUpdate()
    {
        Position = m_updates[0].Position;
        Rotation = m_updates[0].Rotation;
    }

    public static void UpdatePlayers()
    {
        // iterate players
        foreach (var player in Player.ClientPlayerDict.Values)
        {
            // check if enough updates are available
            if (player.PlayerUpdater.m_updates.Count == player.PlayerUpdater.m_totalUpdateCount)
            {
                // interpolate updates or apply latest update
                if (player.PlayerUpdater.m_appSettings.UseInterpolation)
                {
                    player.PlayerUpdater.InterpolateUpdates();
                }
                else
                {
                    player.PlayerUpdater.ApplyUpdate();
                }
            }

            // increase elapsed ticks for the player
            player.PlayerUpdater.m_ticksElapsed++;
        }

        // benchmark
        Player localPlayer = Player.Get(NetworkManager.Instance.Client.Id);
        BenchmarkSampler.Add(localPlayer.PlayerUpdater.Position, localPlayer.PlayerUpdater.Rotation);
    }

    /**
     * Messages
     */

    [MessageHandler((ushort)ServerToClientId.PlayerUpdates)]
    private static void PlayerUpdates(Message message)
    {
        PlayerUpdatesData data = new PlayerUpdatesData(message);

        // abort if tick information is outdated, only newest information needed
        if (data.Tick <= LastPlayerUpdatesReceivedTick) return;
        LastPlayerUpdatesReceivedTick = data.Tick;

        // iterate entries
        foreach (var entry in data.Entries)
        {
            Player player = Player.Get(entry.ClientId);

            // prepare update
            PlayerUpdate playerUpdate = new PlayerUpdate
            {
                Tick = data.Tick,
                Position = entry.Position,
                Rotation = entry.Rotation
            };

            // cache player update
            if (player.PlayerUpdater.m_updates.Count < player.PlayerUpdater.m_totalUpdateCount)
            {
                player.PlayerUpdater.m_updates.Add(playerUpdate);
            }
            else
            {
                // move all entries down and overwrite top entry
                for (int i = 1; i < player.PlayerUpdater.m_totalUpdateCount; i++)
                {
                    player.PlayerUpdater.m_updates[i - 1] = player.PlayerUpdater.m_updates[i];
                }
                player.PlayerUpdater.m_updates[player.PlayerUpdater.m_totalUpdateCount - 1] = playerUpdate;
            }

            // reset ticks elapsed or decrement offset
            if (player.PlayerUpdater.m_bufferOffset == 0)
            {
                player.PlayerUpdater.m_ticksElapsed = 1;
            }
            else
            {
                player.PlayerUpdater.m_bufferOffset--;
            }
        }
    }
}