using FileTtl.BackgroundJobs;
using Microsoft.AspNetCore.Mvc;

namespace FileTtl.Controllers;

[Route("/api/files")]
public class FilesController : Controller
{
    private FileManager FileManager { get; }

    public FilesController(FileManager fileManager)
    {
        FileManager = fileManager;
    }

    [HttpPost("")]
    public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null)
        {
            ModelState.AddModelError("File", $"The file can not be empty.");
            return this.BadRequest(ModelState);
        }
        try
        {
            var fileItem = await this.FileManager.UploadAsync(file, cancellationToken);

            return this.Json(fileItem);
        }
        catch (InvalidOperationException exception)
        {
            return this.BadRequest(exception.Message);
        }
    }

    [HttpGet("")]
    public async Task<IActionResult> DownloadAsync(string hash)
    {
        var downloadInfo = await this.FileManager.GetDownloadInfoAsync(hash);

        if (downloadInfo == null)
        {
            return this.NotFound();
        }

        var stream = System.IO.File.OpenRead(downloadInfo.FilePath);
        if (IsImage(downloadInfo.FileItem.ContentType) || IsPdf(downloadInfo.FileItem.ContentType))
        {
            return this.File(stream, downloadInfo.FileItem.ContentType);
        }
        return this.File(stream, downloadInfo.FileItem.ContentType, downloadInfo.FileItem.FileName);
    }

    private static bool IsImage(string contentType)
    {
        return contentType.Contains("image");
    }

    private static bool IsPdf(string contentType)
    {
        return contentType.Contains("pdf");
    }
}