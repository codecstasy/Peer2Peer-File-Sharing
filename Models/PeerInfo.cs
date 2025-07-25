namespace Peer2Peer_File_Sharing.Models;

/// Lightweight peer information for sharing in gossip protocol

public class PeerInfo
{
    public string Id { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<string> AvailableFiles { get; set; } = new();
}
