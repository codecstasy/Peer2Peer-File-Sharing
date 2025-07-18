namespace Peer2Peer_File_Sharing.Models;

public class Metadata
{
    public required string FileName { get; set; }
    public long TotalSize { get; set; }
    public int ChunkSize { get; set; }
    public List<string> ChunkHashes { get; set; } = new();
}
