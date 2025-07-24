using Microsoft.AspNetCore.Mvc;
using Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;

namespace Peer2Peer_File_Sharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileChunker _fileChunker;

    public FileController(IFileChunker fileChunker)
    {
        _fileChunker = fileChunker;
    }
    
    // Works fine
    [HttpPost("upload")]
    public async Task<ActionResult<ChunkResult>> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        try
        {
            // Save uploaded file temporarily
            var tempPath = Path.GetTempFileName();
            var chunkOutputPath = Path.Combine(Path.GetTempPath(), "p2p_chunks", Guid.NewGuid().ToString());

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Chunk the file
            var result = await _fileChunker.CreateChunksAsync(tempPath, chunkOutputPath);

            // Clean up temp file
            System.IO.File.Delete(tempPath);

            return Ok(new
            {
                Message = "File chunked successfully",
                OriginalFileName = file.FileName,
                FileSize = file.Length,
                Result = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing file: {ex.Message}");
        }
    }

    // Works fine
    [HttpGet("test")]
    public async Task<ActionResult<ChunkResult>> TestChunker()
    {
        try
        {
            // Create a test file
            var testContent = "This is a test file for P2P file sharing. " +
                             "It contains some sample data to verify the chunking functionality works correctly.";
            
            var tempPath = Path.GetTempFileName();
            var chunkOutputPath = Path.Combine(Path.GetTempPath(), "p2p_test_chunks", Guid.NewGuid().ToString());

            await System.IO.File.WriteAllTextAsync(tempPath, testContent);

            // Test the chunker
            var result = await _fileChunker.CreateChunksAsync(tempPath, chunkOutputPath);

            // Clean up temp file
            System.IO.File.Delete(tempPath);

            return Ok(new
            {
                Message = "FileChunker test completed successfully",
                TestFileSize = testContent.Length,
                Result = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Test failed: {ex.Message}");
        }
    }

    // [HttpGet("info/{fileName}")]
    // public ActionResult GetFileInfo(string fileName)
    // {
    //     // This would be used to get info about available files in P2P network
    //     return Ok(new
    //     {
    //         Message = "File info endpoint",
    //         FileName = fileName,
    //         Note = "This endpoint will provide file metadata for P2P sharing"
    //     });
    // }
}
