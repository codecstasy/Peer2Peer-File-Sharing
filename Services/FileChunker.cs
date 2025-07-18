namespace Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

public class FileChunker : IFileChunker
{
    public async Task<Metadata> CreateChunksAsync(string inputFilePath, string chunkOutputPath)
    {
        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException("Input file not found.", inputFilePath);
        }

        Directory.CreateDirectory(chunkOutputPath);

        var metadata = new Metadata
        {
            FileName = Path.GetFileNameWithoutExtension(inputFilePath),
            TotalSize = new FileInfo(inputFilePath).Length,
            ChunkSize = 1024 * 1024 // 1 MB
        };

        using (var fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
        {
            int chunkIndex = 0;
            byte[] buffer = new byte[metadata.ChunkSize];
            while (true)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string chunkFileName = Path.Combine(chunkOutputPath, $"{metadata.FileName}.chunk_{chunkIndex}");
                await File.WriteAllBytesAsync(chunkFileName, buffer.AsSpan(0, bytesRead).ToArray());

                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(buffer.AsSpan(0, bytesRead).ToArray());
                    metadata.ChunkHashes.Add(Convert.ToBase64String(hashBytes));
                }

                chunkIndex++;
            }
            string metadataFilePath = Path.Combine(chunkOutputPath, $"{metadata.FileName}.metadata.json");
            string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataFilePath, json);
        }

        return metadata;
    }
}
