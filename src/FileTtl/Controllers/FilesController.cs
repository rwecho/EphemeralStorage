using FileTtl.BackgroundJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Security.Cryptography;

namespace FileTtl.Controllers;

[Route("/api/files")]
public class FilesController : Controller
{
    public IOptions<FileStorageOptions> Options { get; }
    private FileItemStorage FileItemStorage { get; }

    public FilesController(IOptions<FileStorageOptions> options, FileItemStorage fileItemStorage)
    {
        Options = options;
        FileItemStorage = fileItemStorage;
    }

    [HttpPost("")]
    public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null)
        {
            ModelState.AddModelError("File", $"The file size exceeds the limit.");
            return this.BadRequest(ModelState);
        }

        var totalSize = await this.FileItemStorage.GetTotalSizeAsync();
        var stream = file.OpenReadStream();

        if (this.ReachStorageLimit(totalSize + (ulong)stream.Length))
        {
            return this.BadRequest("The storage limit has been reached, can not upload again.");
        }

        var extensions = Path.GetExtension(file.FileName);
        var hash = ConvertToMd5(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var destFileName = Path.Combine(PathUtilities.Uploads, hash);


        if (System.IO.File.Exists(destFileName))
        {
            var item = await this.FileItemStorage.GetAsync(hash);
            if (item == null)
            {
                ModelState.AddModelError("hash", $"The item with hash it not exists.");
                return this.BadRequest();
            }

            item = item with
            {
                LastModified = DateTime.Now
            };
            await this.FileItemStorage.SetAsync(hash, item);

            return this.Json(item);
        }

        using var destStream = System.IO.File.Open(destFileName, FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(destStream, cancellationToken);
        var fileItem = new FileItem(hash, file.FileName, file.ContentType, extensions, file.Length, DateTime.Now);
        await this.FileItemStorage.SetAsync(hash, fileItem);
        return this.Json(fileItem);
    }

    [HttpGet("")]
    public async Task<IActionResult> DownloadAsync(string hash)
    {
        var fileItem = await this.FileItemStorage.GetAsync(hash);
        var file = Path.Combine(PathUtilities.Uploads, hash);

        if (fileItem == null || !System.IO.File.Exists(file))
        {
            return this.NotFound();
        }

        var stream = System.IO.File.OpenRead(file);
        if (IsImage(fileItem.ContentType) || IsPdf(fileItem.ContentType))
        {
            return this.File(stream, fileItem.ContentType);
        }
        return this.File(stream, fileItem.ContentType, fileItem.FileName);
    }

    private static string ConvertToMd5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return string.Join("", hash.Select(o => o.ToString("X2")));
    }

    private static bool IsImage(string contentType)
    {
        return contentType.Contains("image");
    }

    private static bool IsPdf(string contentType)
    {
        return contentType.Contains("pdf");
    }

    private bool ReachStorageLimit(ulong nextSize)
    {
        return this.Options.Value.MaxStorageLimit > 0 && nextSize > (ulong)this.Options.Value.MaxStorageLimit * 1024 * 1024 * 1024;
    }
}