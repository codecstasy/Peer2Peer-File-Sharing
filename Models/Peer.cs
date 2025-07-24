namespace Peer2Peer_File_Sharing.Models;

public class Peer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<string> AvailableFiles { get; set; } = new();
    public bool IsOnline { get; set; } = true;
    
    // For easy network identification
    public string EndPoint => $"{IpAddress}:{Port}";
}
