namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
using System.Security.Cryptography;
using System.Text.Json;

public class FileAssembler : IFileAssembler
{
    public async Task<bool> AssembleFileAsync(string metadataFilePath, string outputFilePath)
    {
        try
        {
            // Step 1: Load metadata
            if (!File.Exists(metadataFilePath))
            {
                Console.WriteLine($"Metadata file not found: {metadataFilePath}");
                return false;
            }

            var metadataJson = await File.ReadAllTextAsync(metadataFilePath);
            var metadata = JsonSerializer.Deserialize<Metadata>(metadataJson);
            
            if (metadata == null)
            {
                Console.WriteLine("Failed to parse metadata file");
                return false;
            }

            // Step 2: Find chunk directory (same folder as metadata)
            var chunkDirectory = Path.GetDirectoryName(metadataFilePath);
            
            // Step 3: Verify all chunks exist and are valid
            Console.WriteLine($"Verifying {metadata.ChunkCount} chunks...");
            
            for (int i = 0; i < metadata.ChunkCount; i++)
            {
                var chunkFileName = Path.Combine(chunkDirectory, $"{metadata.FileName}.chunk_{i}");
                
                // Check if chunk file exists
                if (!File.Exists(chunkFileName))
                {
                    Console.WriteLine($"Missing chunk: {chunkFileName}");
                    return false;
                }
                
                // Verify chunk integrity
                var chunkData = await File.ReadAllBytesAsync(chunkFileName);
                var actualHash = ComputeHash(chunkData);
                var expectedHash = metadata.ChunkHashes[i];
                
                if (actualHash != expectedHash)
                {
                    Console.WriteLine($"Chunk {i} is corrupted! Expected: {expectedHash}, Got: {actualHash}");
                    return false;
                }
                
                Console.WriteLine($"Chunk {i} verified ✓");
            }
            
            // Step 4: Assemble the file
            Console.WriteLine($"Assembling file: {outputFilePath}");
            
            var outputStream = new FileStream(outputFilePath, FileMode.Create);
            
            for (int i = 0; i < metadata.ChunkCount; i++)
            {
                var chunkFileName = Path.Combine(chunkDirectory, $"{metadata.FileName}.chunk_{i}");
                var chunkData = await File.ReadAllBytesAsync(chunkFileName);
                
                await outputStream.WriteAsync(chunkData);
                Console.WriteLine($"Added chunk {i} to file");
            }
            
            outputStream.Close();
            
            // Step 5: Final verification
            var finalFileInfo = new FileInfo(outputFilePath);
            if (finalFileInfo.Length == metadata.TotalSize)
            {
                Console.WriteLine($"✅ File assembled successfully! Size: {finalFileInfo.Length} bytes");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ File size mismatch! Expected: {metadata.TotalSize}, Got: {finalFileInfo.Length}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assembling file: {ex.Message}");
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
