namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

public class FileChunker : IFileChunker
{
    private readonly ILogger<FileChunker> _logger;

    public FileChunker(ILogger<FileChunker> logger)
    {
        _logger = logger;
    }
    public async Task<ChunkResult> CreateChunksAsync(string inputFilePath, string chunkOutputPath)
    {
        _logger.LogInformation("Starting file chunking process for {InputFilePath}", inputFilePath);
        
        try
        {
            // Validate input file exists
            if (!File.Exists(inputFilePath))
            {
                _logger.LogError("Input file not found: {InputFilePath}", inputFilePath);
                throw new FileNotFoundException("Input file not found.", inputFilePath);
            }

            // Create output directory
            _logger.LogDebug("Creating output directory: {ChunkOutputPath}", chunkOutputPath);
            Directory.CreateDirectory(chunkOutputPath);

            // Set up metadata
            var fileInfo = new FileInfo(inputFilePath);
            var metadata = new Metadata
            {
                FileName = Path.GetFileNameWithoutExtension(inputFilePath),
                TotalSize = fileInfo.Length,
                ChunkSize = 1024 * 1024 // 1 MB chunks
            };

            var estimatedChunks = (int)Math.Ceiling((double)metadata.TotalSize / metadata.ChunkSize);
            _logger.LogInformation("File metadata - Name: {FileName}, Size: {FileSize} bytes, Estimated chunks: {EstimatedChunks}", 
                metadata.FileName, metadata.TotalSize, estimatedChunks);

            // Initialize result object
            var result = new ChunkResult
            {
                Metadata = metadata,
                ChunkDirectory = chunkOutputPath
            };

            // Read and split file into chunks
            _logger.LogInformation("Starting chunking process with chunk size: {ChunkSize} bytes", metadata.ChunkSize);
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
                
                _logger.LogDebug("Created chunk {ChunkIndex}: {ChunkSize} bytes, Hash: {Hash}", 
                    chunkIndex, chunkData.Length, hash);
                
                chunkIndex++;
            }
            
            // Update metadata with actual chunk count
            _logger.LogInformation("Successfully created {ChunkCount} chunks", metadata.ChunkCount);
            
            // Save metadata file
            var metadataPath = Path.Combine(chunkOutputPath, $"{metadata.FileName}.metadata.json");
            result.MetadataFilePath = metadataPath;
            
            _logger.LogDebug("Saving metadata to: {MetadataPath}", metadataPath);
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, json);

            _logger.LogInformation("✅ File chunking completed successfully! Created {ChunkCount} chunks in {OutputDirectory}", 
                metadata.ChunkCount, chunkOutputPath);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file chunking process for {InputFilePath}: {ErrorMessage}", 
                inputFilePath, ex.Message);
            throw;
        }
    }
}
