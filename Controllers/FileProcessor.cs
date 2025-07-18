using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Peer2Peer_File_Sharing.Services;

namespace Peer2Peer_File_Sharing.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileProcessor : ControllerBase
{
    private readonly IFileChunker _fileChunker;
    public FileProcessor(IFileChunker fileChunker)
    {
        _fileChunker = fileChunker;
    }

    // For test -- to prepare a file
    [HttpGet("prepare-file")]
    public async Task<IActionResult> PrepareFile([FromQuery] string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest("File path is required.");
        }

        try
        {
            await _fileChunker.CreateChunksAsync(filePath);
            return Ok("File chunking started successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}

