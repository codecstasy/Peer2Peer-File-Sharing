namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
public interface IFileChunker
{
    Task<Metadata> CreateChunksAsync(string inputFilePath, string chunkOutputPath);
}
