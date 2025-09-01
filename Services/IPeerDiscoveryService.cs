namespace Peer2Peer_File_Sharing.Services;

using Models;


/// Service interface for managing peer discovery and network state in P2P system

public interface IPeerDiscoveryService
{
    
    /// Register a new peer in the network
    
    Task<PeerRegistrationResponse> RegisterPeerAsync(PeerRegistrationRequest request);
    
    
    /// Unregister a peer from the network
    
    Task<bool> UnregisterPeerAsync(string peerId);
    
    
    /// Update peer's heartbeat and status
    
    Task<HeartbeatResponse> UpdateHeartbeatAsync(HeartbeatRequest request);
    
    
    /// Get all active peers in the network
    
    Task<List<PeerInfo>> GetActivePeersAsync();
    
    
    /// Get specific peers by their IDs
    
    Task<List<PeerInfo>> GetPeersByIdsAsync(List<string> peerIds);
    
    
    /// Announce that a peer has specific chunks of a file
    
    Task<ChunkAnnounceResponse> AnnounceChunksAsync(ChunkAnnounceRequest request);
    
    
    /// Find peers that have specific chunks of a file
    
    Task<ChunkPeerDiscoveryResponse> DiscoverChunkPeersAsync(ChunkPeerDiscoveryRequest request);
    
    
    /// Get distributed metadata for a file
    
    Task<DistributedMetadata?> GetFileMetadataAsync(string fileId);
    
    
    /// Update distributed metadata for a file
    
    Task<bool> UpdateFileMetadataAsync(DistributedMetadata metadata);
    
    
    /// Remove inactive/offline peers from the network
    
    Task<int> CleanupInactivePeersAsync(TimeSpan inactiveThreshold);
    
    
    /// Get network statistics
    
    Task<NetworkStats> GetNetworkStatsAsync();
}
