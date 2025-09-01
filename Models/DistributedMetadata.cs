namespace Peer2Peer_File_Sharing.Models;


/// Enhanced metadata for P2P file sharing that tracks which peers have which chunks

public class DistributedMetadata : Metadata
{
    
    /// Maps chunk index to list of peer IDs that have that chunk
    
    public Dictionary<int, List<string>> ChunkToPeers { get; set; } = new();
    
    
    /// File ID for uniquely identifying files across the network
    
    public string FileId { get; set; } = string.Empty;
    
    
    /// When this metadata was created
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    /// Last time this metadata was updated
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    
    /// Priority level for file replication (higher = more important)
    
    public int ReplicationPriority { get; set; } = 1;
    
    
    /// Add a peer as having a specific chunk
    
    public void AddChunkToPeer(int chunkIndex, string peerId)
    {
        if (!ChunkToPeers.ContainsKey(chunkIndex))
        {
            ChunkToPeers[chunkIndex] = new List<string>();
        }
        
        if (!ChunkToPeers[chunkIndex].Contains(peerId))
        {
            ChunkToPeers[chunkIndex].Add(peerId);
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    
    /// Remove a peer from having a specific chunk
    
    public void RemoveChunkFromPeer(int chunkIndex, string peerId)
    {
        if (ChunkToPeers.ContainsKey(chunkIndex))
        {
            ChunkToPeers[chunkIndex].Remove(peerId);
            LastUpdated = DateTime.UtcNow;
            
            if (ChunkToPeers[chunkIndex].Count == 0)
            {
                ChunkToPeers.Remove(chunkIndex);
            }
        }
    }
    
    
    /// Get peers that have a specific chunk
    
    public List<string> GetPeersWithChunk(int chunkIndex)
    {
        return ChunkToPeers.GetValueOrDefault(chunkIndex, new List<string>());
    }
    
    
    /// Get all chunks that a peer has
    
    public List<int> GetChunksForPeer(string peerId)
    {
        return ChunkToPeers
            .Where(kvp => kvp.Value.Contains(peerId))
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    
    /// Get chunks with the fewest replicas (rarest chunks)
    
    public List<int> GetRarestChunks(int topN = 5)
    {
        return ChunkToPeers
            .OrderBy(kvp => kvp.Value.Count)
            .Take(topN)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
