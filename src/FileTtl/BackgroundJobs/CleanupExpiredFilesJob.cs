using FileTtl.Controllers;
using Microsoft.Extensions.Options;
using Quartz;

namespace FileTtl.BackgroundJobs;

public class CleanupExpiredFilesJob : IJob
{
    public CleanupExpiredFilesJob(ILogger<CleanupExpiredFilesJob> logger,
        FileItemStorage fileItemStorage,
        IOptions<FileStorageOptions> options)
    {
        Logger = logger;
        FileItemStorage = fileItemStorage;
        Options = options;
    }

    private ILogger<CleanupExpiredFilesJob> Logger { get; }
    private FileItemStorage FileItemStorage { get; }
    private IOptions<FileStorageOptions> Options { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        this.Logger.LogInformation("Cleaning up expired files.");

        var validHours = Options.Value.ValidHours;
        var items = (await this.FileItemStorage.GetItemsAsync()).Where(o => DateTime.Now.Subtract(o.LastModified).TotalHours > validHours)
            .ToArray();

        foreach (var item in items)
        {
            var destFileName = Path.Combine(PathUtilities.Uploads, item.Hash);
            File.Delete(destFileName);
            await this.FileItemStorage.RemoveAsync(item.Hash);
        }

        this.Logger.LogInformation($"{items.Length} files were cleaned up");
    }
}
