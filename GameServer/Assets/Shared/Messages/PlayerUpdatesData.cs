using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerUpdatesData
{
    public long Tick;
    public List<Entry> Entries;

    public struct Entry
    {
        public ushort ClientId;
        public Vector3 Position;
        public Vector2 Rotation;
    }

    public PlayerUpdatesData(Message message)
    {
        Tick = message.GetLong();
        Entries = new List<Entry>(message.GetInt());

        for (int i = 0; i < Entries.Capacity; i++)
        {
            Entries.Add(new Entry
            {
                ClientId = message.GetUShort(),
                Position = message.GetVector3(),
                Rotation = message.GetVector2()
            });
        }
    }

    public Message CreateMessage()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.PlayerUpdates);
        message.AddLong(Tick);
        message.AddInt(Entries.Count);

        foreach (var entry in Entries)
        {
            message.AddUShort(entry.ClientId);
            message.AddVector3(entry.Position);
            message.AddVector2(entry.Rotation);
        }

        return message;
    }
}