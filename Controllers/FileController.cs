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
    private readonly ILogger<FileController> _logger;

    public FileController(IFileChunker fileChunker, IFileAssembler fileAssembler, ILogger<FileController> logger)
    {
        _fileChunker = fileChunker;
        _fileAssembler = fileAssembler;
        _logger = logger;
    }
    
    // Works fine
    [HttpPost("upload")]
    public async Task<ActionResult<ChunkResult>> UploadFile(IFormFile file)
    {
        _logger.LogInformation("Upload request received for file: {FileName}, Size: {FileSize} bytes", 
            file?.FileName ?? "unknown", file?.Length ?? 0);
            
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload request rejected: No file uploaded or file is empty");
            return BadRequest("No file uploaded");
        }

        try
        {
            // Save uploaded file temporarily
            var tempPath = Path.GetTempFileName();
            var chunkOutputPath = Path.Combine(Path.GetTempPath(), "p2p_chunks", Guid.NewGuid().ToString());
            
            _logger.LogDebug("Saving uploaded file temporarily to: {TempPath}", tempPath);
            _logger.LogDebug("Chunk output directory: {ChunkOutputPath}", chunkOutputPath);

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Chunk the file
            var result = await _fileChunker.CreateChunksAsync(tempPath, chunkOutputPath);

            // Clean up temp file
            _logger.LogDebug("Cleaning up temporary file: {TempPath}", tempPath);
            System.IO.File.Delete(tempPath);

            _logger.LogInformation("✅ File upload and chunking completed successfully for {FileName}", file.FileName);
            
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
            _logger.LogError(ex, "Error processing uploaded file {FileName}: {ErrorMessage}", 
                file?.FileName ?? "unknown", ex.Message);
            return StatusCode(500, $"Error processing file: {ex.Message}");
        }
    }

    // Test the assembler with existing chunks
    [HttpPost("assemble")]
    public async Task<IActionResult> Assembler([FromBody] AssembleRequest request)
    {
        _logger.LogInformation("Assembly request received - Metadata: {MetadataPath}, Output: {OutputPath}", 
            request?.MetadataFilePath ?? "unknown", request?.OutputFilePath ?? "unknown");
            
        if (request == null || string.IsNullOrEmpty(request.MetadataFilePath) || string.IsNullOrEmpty(request.OutputFilePath))
        {
            _logger.LogWarning("Assembly request rejected: Invalid request parameters");
            return BadRequest("Invalid request parameters");
        }
            
        try
        {
            var success = await _fileAssembler.AssembleFileAsync(
                request.MetadataFilePath, 
                request.OutputFilePath
            );
            
            if (success)
            {
                var fileInfo = new FileInfo(request.OutputFilePath);
                _logger.LogInformation("✅ File assembly completed successfully - Output: {OutputPath}, Size: {FileSize} bytes", 
                    request.OutputFilePath, fileInfo.Length);
                    
                return Ok(new 
                { 
                    Message = "File assembled successfully!", 
                    OutputPath = request.OutputFilePath,
                    FileSize = fileInfo.Length
                });
            }
            else
            {
                _logger.LogError("File assembly failed for metadata: {MetadataPath}", request.MetadataFilePath);
                return BadRequest("Failed to assemble file - check logs for details");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Assembly error for metadata {MetadataPath}: {ErrorMessage}", 
                request?.MetadataFilePath ?? "unknown", ex.Message);
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
