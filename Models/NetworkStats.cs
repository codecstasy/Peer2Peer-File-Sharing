namespace Peer2Peer_File_Sharing.Models;

/// Network statistics model for monitoring P2P network health
public class NetworkStats
{
    public int TotalPeers { get; set; }
    public int ActivePeers { get; set; }
    public int TotalFiles { get; set; }
    public int TotalChunks { get; set; }
    public double AverageReplicationFactor { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
