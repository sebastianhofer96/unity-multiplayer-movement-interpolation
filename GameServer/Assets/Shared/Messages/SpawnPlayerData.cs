using RiptideNetworking;
using UnityEngine;

public struct SpawnPlayerData
{
    public ushort ClientId;
    public Vector3 SpawnPosition;
    public long SynchronizationTick;

    public SpawnPlayerData(Message message)
    {
        ClientId = message.GetUShort();
        SpawnPosition = message.GetVector3();
        SynchronizationTick = message.GetLong();
    }

    public Message CreateMessage()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.SpawnPlayer);
        message.AddUShort(ClientId);
        message.AddVector3(SpawnPosition);
        message.AddLong(SynchronizationTick);
        return message;
    }
}