namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
using System.Security.Cryptography;
using System.Text.Json;

public class FileAssembler : IFileAssembler
{
    private readonly ILogger<FileAssembler> _logger;

    public FileAssembler(ILogger<FileAssembler> logger)
    {
        _logger = logger;
    }
    public async Task<bool> AssembleFileAsync(string metadataFilePath, string outputFilePath)
    {
        try
        {
            // Step 1: Load metadata
            if (!File.Exists(metadataFilePath))
            {
                _logger.LogError("Metadata file not found: {MetadataFilePath}", metadataFilePath);
                return false;
            }

            var metadataJson = await File.ReadAllTextAsync(metadataFilePath);
            var metadata = JsonSerializer.Deserialize<Metadata>(metadataJson);
            
            if (metadata == null)
            {
                _logger.LogError("Failed to parse metadata file");
                return false;
            }

            // Step 2: Find chunk directory (same folder as metadata)
            var chunkDirectory = Path.GetDirectoryName(metadataFilePath);
            
            // Step 3: Verify all chunks exist and are valid
            _logger.LogInformation("Verifying {ChunkCount} chunks...", metadata.ChunkCount);
            
            for (int i = 0; i < metadata.ChunkCount; i++)
            {
                var chunkFileName = Path.Combine(chunkDirectory, $"{metadata.FileName}.chunk_{i}");
                
                // Check if chunk file exists
                if (!File.Exists(chunkFileName))
                {
                    _logger.LogError("Missing chunk: {ChunkFileName}", chunkFileName);
                    return false;
                }
                
                // Verify chunk integrity
                var chunkData = await File.ReadAllBytesAsync(chunkFileName);
                var actualHash = ComputeHash(chunkData);
                var expectedHash = metadata.ChunkHashes[i];
                
                if (actualHash != expectedHash)
                {
                    _logger.LogError("Chunk {ChunkIndex} is corrupted! Expected: {ExpectedHash}, Got: {ActualHash}", i, expectedHash, actualHash);
                    return false;
                }
                
                _logger.LogDebug("Chunk {ChunkIndex} verified ✓", i);
            }
            
            // Step 4: Assemble the file
            _logger.LogInformation("Assembling file: {OutputFilePath}", outputFilePath);
            
            var outputStream = new FileStream(outputFilePath, FileMode.Create);
            
            for (int i = 0; i < metadata.ChunkCount; i++)
            {
                var chunkFileName = Path.Combine(chunkDirectory, $"{metadata.FileName}.chunk_{i}");
                var chunkData = await File.ReadAllBytesAsync(chunkFileName);
                
                await outputStream.WriteAsync(chunkData);
                _logger.LogDebug("Added chunk {ChunkIndex} to file", i);
            }
            
            outputStream.Close();
            
            // Step 5: Final verification
            var finalFileInfo = new FileInfo(outputFilePath);
            if (finalFileInfo.Length == metadata.TotalSize)
            {
                _logger.LogInformation("✅ File assembled successfully! Size: {FileSize} bytes", finalFileInfo.Length);
                return true;
            }
            else
            {
                _logger.LogError("❌ File size mismatch! Expected: {ExpectedSize}, Got: {ActualSize}", metadata.TotalSize, finalFileInfo.Length);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling file: {ErrorMessage}", ex.Message);
            return false;
        }
    }
    
    private string ComputeHash(byte[] chunkData)
    {
        using var sha256 = SHA256.Create();
        var hash = Convert.ToBase64String(sha256.ComputeHash(chunkData));
        return hash;
    }
}
