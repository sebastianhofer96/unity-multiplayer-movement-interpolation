using RiptideNetworking;

public struct AckPlayerIntentionData
{
    public long AcknowledgedTick;

    public AckPlayerIntentionData(Message message)
    {
        AcknowledgedTick = message.GetLong();
    }

    public Message CreateMessage()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.AckPlayerIntention);
        message.AddLong(AcknowledgedTick);
        return message;
    }
}