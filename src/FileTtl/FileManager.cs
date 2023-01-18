using FileTtl.Controllers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FileTtl.BackgroundJobs;

public class FileManager
{
	public FileManager(IOptions<FileStorageOptions> options, 
        FileItemStorage fileItemStorage)
	{
        Options = options;
        FileItemStorage = fileItemStorage;
    }

    private IOptions<FileStorageOptions> Options { get; }
    private FileItemStorage FileItemStorage { get; }

    public async Task<FileItem> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var totalSize = await this.FileItemStorage.GetTotalSizeAsync();
        var stream = file.OpenReadStream();

        if (this.ReachStorageLimit(totalSize + (ulong)stream.Length))
        {
            throw new InvalidOperationException("The storage limit has been reached, can not upload again.");
        }

        var extensions = Path.GetExtension(file.FileName);
        var hash = ConvertToMd5(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var destFileName = Path.Combine(PathUtilities.Uploads, hash);

        if (File.Exists(destFileName))
        {
            var item = await this.FileItemStorage.GetAsync(hash);
            if (item == null)
            {
                throw new InvalidOperationException("The item with hash is not exists.");
            }

            item = item with
            {
                LastModified = DateTime.Now
            };

            await this.FileItemStorage.SetAsync(hash, item);
            return item;
        }

        using var destStream = File.Open(destFileName, FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(destStream, cancellationToken);
        var fileItem = new FileItem(hash, file.FileName, file.ContentType, extensions, file.Length, DateTime.Now);
        await this.FileItemStorage.SetAsync(hash, fileItem);

        return fileItem;
    }

    public async Task<DownloadInfo?> GetDownloadInfoAsync(string hash)
    {
        var fileItem = await this.FileItemStorage.GetAsync(hash);
        var file = Path.Combine(PathUtilities.Uploads, hash);
        if (fileItem == null || !File.Exists(file))
        {
            return null;
        }

        return new(fileItem, file);
    }

    private static string ConvertToMd5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return string.Join("", hash.Select(o => o.ToString("X2")));
    }

    private bool ReachStorageLimit(ulong nextSize)
    {
        return this.Options.Value.MaxStorageLimit > 0 && nextSize > (ulong)this.Options.Value.MaxStorageLimit * 1024 * 1024 * 1024;
    }
}
