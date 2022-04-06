public struct PlayerIntention
{
    public long Tick; // intentionally missing { get; set; } to ignore for csv export
    public bool MoveForward { get; set; }
    public bool MoveBackward { get; set; }
    public bool MoveLeft { get; set; }
    public bool MoveRight { get; set; }
    public float LookHorizontal { get; set; }
    public float LookVertical { get; set; }

    public PlayerIntention(PlayerIntentionData.Entry entry, long baseTick)
    {
        Tick = baseTick + entry.TickOffset;
        bool[] moveBools = Utils.ByteToBooleans(entry.Movement);
        MoveForward = moveBools[0];
        MoveBackward = moveBools[1];
        MoveLeft = moveBools[2];
        MoveRight = moveBools[3];
        LookHorizontal = entry.LookHorizontal;
        LookVertical = entry.LookVertical;
    }

    public PlayerIntentionData.Entry CreateEntry(long baseTick)
    {
        return new PlayerIntentionData.Entry
        {
            TickOffset = (byte)(Tick - baseTick),
            Movement = Utils.BooleansToByte(new bool[] { MoveForward, MoveBackward, MoveLeft, MoveRight }),
            LookHorizontal = LookHorizontal,
            LookVertical = LookVertical
        };
    }
}