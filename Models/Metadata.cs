namespace Peer2Peer_File_Sharing.Models;
using System.Collections.Generic;

public class Metadata
{
    public string FileName { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public int ChunkSize { get; set; }
    public List<string> ChunkHashes { get; set; } = new();
    public int ChunkCount => ChunkHashes.Count;
}
