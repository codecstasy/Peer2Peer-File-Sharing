namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;

public interface IFileChunker
{
    Task<ChunkResult> CreateChunksAsync(string inputFilePath, string chunkOutputPath);
}
