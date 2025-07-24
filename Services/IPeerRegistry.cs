namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;

public interface IPeerRegistry
{
    Task RegisterPeerAsync(Peer peer);
    Task<List<Peer>> GetAvailablePeersAsync();
    Task<List<Peer>> GetPeersWithFileAsync(string fileName);
    Task UpdatePeerLastSeenAsync(string peerId);
    Task UnregisterPeerAsync(string peerId);
    Task<bool> IsPeerOnlineAsync(string peerId);
}
