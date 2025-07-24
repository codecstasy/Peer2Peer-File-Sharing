namespace Peer2Peer_File_Sharing.Models;
using System;
using System.Collections.Generic;

public class ChunkResult
{
    public Metadata Metadata { get; set; } = new();
    public string ChunkDirectory { get; set; } = string.Empty;
    public List<string> ChunkFiles { get; set; } = new();
    public string MetadataFilePath { get; set; } = string.Empty;
    
    // Additional useful info for P2P
    public int TotalChunks => ChunkFiles.Count;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // public string OriginalFilePath { get; set; } = string.Empty;
}
