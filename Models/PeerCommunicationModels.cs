namespace Peer2Peer_File_Sharing.Models;


/// Request model for peer registration - reuses existing PeerInfo model

public class PeerRegistrationRequest
{
    public PeerInfo PeerInfo { get; set; } = new();
}


/// Response model for peer registration

public class PeerRegistrationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PeerInfo> KnownPeers { get; set; } = new();
}


/// Request model for discovering peers that have specific chunks

public class ChunkPeerDiscoveryRequest
{
    public string FileId { get; set; } = string.Empty;
    public List<int> ChunkIndices { get; set; } = new();
}


/// Response model for chunk peer discovery

public class ChunkPeerDiscoveryResponse
{
    public Dictionary<int, List<PeerInfo>> ChunkToPeers { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}


/// Request model for downloading a chunk from a peer

public class ChunkDownloadRequest
{
    public string FileId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string RequestingPeerId { get; set; } = string.Empty;
}


/// Response model for chunk download

public class ChunkDownloadResponse
{
    public bool Success { get; set; }
    public byte[] ChunkData { get; set; } = Array.Empty<byte>();
    public string ChunkHash { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}


/// Request model for announcing that a peer has specific chunks

public class ChunkAnnounceRequest
{
    public string PeerId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public List<int> ChunkIndices { get; set; } = new();
}


/// Response model for chunk announcement

public class ChunkAnnounceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}


/// Request model for peer heartbeat/status update - reuses existing PeerInfo model

public class HeartbeatRequest
{
    public PeerInfo PeerInfo { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}


/// Response model for heartbeat

public class HeartbeatResponse
{
    public bool Success { get; set; }
    public List<PeerInfo> RecentlyJoinedPeers { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
