
// ids for messages sent from server to client
public enum ServerToClientId : ushort
{
    SpawnPlayer,
    PlayerUpdates,
    AckPlayerIntention
}

// ids for messages sent from client to server
public enum ClientToServerId : ushort
{
    PlayerIntention
}