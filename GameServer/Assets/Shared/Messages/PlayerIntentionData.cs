using RiptideNetworking;
using System.Collections.Generic;

public struct PlayerIntentionData
{
    public long BaseTick;
    public List<Entry> Entries;

    public struct Entry
    {
        public byte TickOffset;
        public byte Movement;
        public float LookHorizontal;
        public float LookVertical;
    }

    public PlayerIntentionData(Message message)
    {
        BaseTick = message.GetLong();
        Entries = new List<Entry>(message.GetInt());

        for (int i = 0; i < Entries.Capacity; i++)
        {
            Entries.Add(new Entry
            {
                TickOffset = message.GetByte(),
                Movement = message.GetByte(),
                LookHorizontal = message.GetFloat(),
                LookVertical = message.GetFloat()
            });
        }
    }

    public Message CreateMessage()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.PlayerIntention);
        message.AddLong(BaseTick);
        message.AddInt(Entries.Count);

        foreach (var entry in Entries)
        {
            message.AddByte(entry.TickOffset);
            message.AddByte(entry.Movement);
            message.AddFloat(entry.LookHorizontal);
            message.AddFloat(entry.LookVertical);
        }

        return message;
    }
}