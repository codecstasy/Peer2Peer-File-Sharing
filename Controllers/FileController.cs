using Microsoft.AspNetCore.Mvc;
using Peer2Peer_File_Sharing.Services;
using Peer2Peer_File_Sharing.Models;

namespace Peer2Peer_File_Sharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileChunker _fileChunker;
    private readonly IFileAssembler _fileAssembler;

    public FileController(IFileChunker fileChunker, IFileAssembler fileAssembler)
    {
        _fileChunker = fileChunker;
        _fileAssembler = fileAssembler;
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

    // Test the assembler with existing chunks
    [HttpPost("assemble")]
    public async Task<IActionResult> Assembler([FromBody] AssembleRequest request)
    {
        try
        {
            var success = await _fileAssembler.AssembleFileAsync(
                request.MetadataFilePath, 
                request.OutputFilePath
            );
            
            if (success)
            {
                var fileInfo = new FileInfo(request.OutputFilePath);
                return Ok(new 
                { 
                    Message = "File assembled successfully!", 
                    OutputPath = request.OutputFilePath,
                    FileSize = fileInfo.Length
                });
            }
            else
            {
                return BadRequest("Failed to assemble file - check console for details");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Assembly error: {ex.Message}");
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
