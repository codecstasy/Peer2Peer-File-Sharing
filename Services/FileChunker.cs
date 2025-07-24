namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

public class FileChunker : IFileChunker
{
    public async Task<ChunkResult> CreateChunksAsync(string inputFilePath, string chunkOutputPath)
    {
        // Validate input file exists
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file not found.", inputFilePath);

        // Create output directory
        Directory.CreateDirectory(chunkOutputPath);

        // Set up metadata
        var metadata = new Metadata
        {
            FileName = Path.GetFileNameWithoutExtension(inputFilePath),
            TotalSize = new FileInfo(inputFilePath).Length,
            ChunkSize = 1024 * 1024 // 1 MB chunks
        };

        // Initialize result object
        var result = new ChunkResult
        {
            Metadata = metadata,
            ChunkDirectory = chunkOutputPath
        };

        // Read and split file into chunks
        using var fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[metadata.ChunkSize];
        int chunkIndex = 0;
        
        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
        {
            // Get only the actual data (not the full buffer)
            var chunkData = buffer[..bytesRead];
            
            // Save chunk to file
            var chunkFileName = Path.Combine(chunkOutputPath, $"{metadata.FileName}.chunk_{chunkIndex}");
            await File.WriteAllBytesAsync(chunkFileName, chunkData);
            
            // Track chunk file paths
            result.ChunkFiles.Add(chunkFileName);
            
            // Generate and store hash for integrity
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(chunkData));
            metadata.ChunkHashes.Add(hash);
            
            chunkIndex++;
        }
        
        // Save metadata file
        var metadataPath = Path.Combine(chunkOutputPath, $"{metadata.FileName}.metadata.json");
        result.MetadataFilePath = metadataPath;
        
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataPath, json);

        return result;
    }
}
