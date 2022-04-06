using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_gravity = 9.81f;
    [SerializeField] private float m_movementSpeed = 5f;
    [SerializeField] private float m_cameraVerticalClampAngle = 85f;
    [SerializeField] private Transform m_cameraTransform;

    public static Dictionary<ushort, List<PlayerIntention>> IntentionHistories = new Dictionary<ushort, List<PlayerIntention>>();
    public long ExpectedTick { get; set; } = -1;
    private static long LastStateTickSent = -1;

    private Player m_player;
    private CharacterController m_characterController;
    private float m_velocityY;
    private Vector2 m_rotation;

    public Vector3 Position => transform.position;

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
        m_player = GetComponent<Player>();
        m_characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // draw look direction
        Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward * 2f, Color.green);
    }

    private Vector3 FlattenVector3(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public PlayerUpdatesData.Entry GetPlayerData()
    {
        return new PlayerUpdatesData.Entry
        {
            ClientId = m_player.ClientId,
            Position = Position,
            Rotation = Rotation
        };
    }

    public void Look(PlayerIntention playerInput)
    {
        // apply horizontal look
        float newRotationY = Rotation.y + playerInput.LookHorizontal * Time.fixedDeltaTime;

        // apply vertical look
        float newRotationX = Rotation.x - playerInput.LookVertical * Time.fixedDeltaTime;
        newRotationX = Mathf.Clamp(newRotationX, -m_cameraVerticalClampAngle, m_cameraVerticalClampAngle);

        Rotation = new Vector2(newRotationX, newRotationY);
    }

    public void Move(PlayerIntention playerInput)
    {
        // direction of input
        Vector2 inputDirection = Vector2.zero;
        if (playerInput.MoveForward)
            inputDirection.y += 1;
        if (playerInput.MoveBackward)
            inputDirection.y -= 1;
        if (playerInput.MoveLeft)
            inputDirection.x -= 1;
        if (playerInput.MoveRight)
            inputDirection.x += 1;

        // direction of player
        Vector3 moveDirection = Vector3.Normalize(m_cameraTransform.right * inputDirection.x + Vector3.Normalize(FlattenVector3(m_cameraTransform.forward)) * inputDirection.y);
        moveDirection *= m_movementSpeed * Time.fixedDeltaTime;

        // gravity
        if (m_characterController.isGrounded)
        {
            m_velocityY = 0f;
        }
        m_velocityY += m_gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveDirection.y = m_velocityY;

        // apply
        m_characterController.Move(moveDirection);
    }

    public long IntentionHistoryReceived(PlayerIntentionData data)
    {
        long tickToAck = -1;

        foreach (var entry in data.Entries)
        {
            PlayerIntention playerIntention = new PlayerIntention(entry, data.BaseTick);

            if (playerIntention.Tick == ExpectedTick)
            {
                // add input to history for player
                IntentionHistories[m_player.ClientId].Add(playerIntention);
                
                // a new tick can be acknowledged
                tickToAck = playerIntention.Tick;

                // increment expected tick
                ExpectedTick++;
            }
        }

        return tickToAck;
    }

    public static void SendPlayerUpdates()
    {
        // only send player updates if game state has changed
        if (LastStateTickSent == GameStateManager.CurrentStateTick) return;

        Debug.Log($"Sending player updates for game state with tick {GameStateManager.CurrentStateTick}");

        PlayerUpdatesData playerUpdatesData = new PlayerUpdatesData
        {
            Tick = GameStateManager.CurrentStateTick,
            Entries = new List<PlayerUpdatesData.Entry>()
        };

        foreach (var player in Player.ClientPlayerDict.Values)
        {
            playerUpdatesData.Entries.Add(player.PlayerController.GetPlayerData());
        }

        // send game state to all clients
        NetworkManager.Instance.Server.SendToAll(playerUpdatesData.CreateMessage());

        // update tick of last state sent
        LastStateTickSent = GameStateManager.CurrentStateTick;
    }
}