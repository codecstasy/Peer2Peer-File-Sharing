namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;

public interface IFileAssembler
{
    Task<bool> AssembleFileAsync(string metadataFilePath, string outputFilePath);
}
