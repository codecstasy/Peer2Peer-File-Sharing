namespace Peer2Peer_File_Sharing.Models;

public class Peer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<string> AvailableFiles { get; set; } = new();
    public bool IsOnline { get; set; } = true;
    
    // Peer discovery properties
    public List<PeerInfo> KnownPeers { get; set; } = new();
    public DateTime LastPeerListUpdateDateTime { get; set; } = DateTime.UtcNow;
    public int MaxKnownPeers { get; set; } = 50; // Limit to control growth
    public string EndPoint => $"{IpAddress}:{Port}";
    
    // Convert self to PeerInfo for lightweight sharing
    public PeerInfo ToPeerInfo()
    {
        PeerInfo peerInfo = new PeerInfo();
        peerInfo.Id = Id;
        peerInfo.IpAddress = IpAddress;
        peerInfo.Port = Port;
        peerInfo.LastSeen = LastSeen;
        peerInfo.AvailableFiles = new List<string>(AvailableFiles);
        return peerInfo;
    }
    
    // Add a peer to known peers list (with deduplication)
    public void AddKnownPeer(PeerInfo peerInfo)
    {
        if (peerInfo.Id == Id) return; // Don't add self
        
        var existing = KnownPeers.FirstOrDefault(p => p.Id == peerInfo.Id);
        if (existing != null)
        {
            // Update existing peer info
            existing.LastSeen = peerInfo.LastSeen;
            existing.AvailableFiles = peerInfo.AvailableFiles;
            existing.IpAddress = peerInfo.IpAddress;
            existing.Port = peerInfo.Port;
        }
        else
        {
            // Add new peer, but respect the limit
            if (KnownPeers.Count >= MaxKnownPeers)
            {
                // Remove oldest peer
                var oldest = KnownPeers.OrderBy(p => p.LastSeen).First();
                KnownPeers.Remove(oldest);
            }
            KnownPeers.Add(peerInfo);
        }
        
        LastPeerListUpdateDateTime = DateTime.UtcNow;
    }
    
    // Get random subset of known peers for gossip
    public List<PeerInfo> GetRandomPeers(int count = 10)
    {
        var activePeers = KnownPeers
            .Where(p => (DateTime.UtcNow - p.LastSeen).TotalMinutes < 30) // Only recently seen peers
            .ToList();
            
        return activePeers
            .OrderBy(x => Guid.NewGuid())
            .Take(count)
            .ToList();
    }
}
