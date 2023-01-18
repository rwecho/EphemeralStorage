using System.Text.Json;

namespace FileTtl.Controllers;

public class FileItemStorage
{
    public FileItemStorage()
    {
        this.FilePath = Path.Combine(PathUtilities.AppData, "FileItems.json");
        this.LoadTask = Task.Run(this.Load);
    }
    private string FilePath { get; }
    private readonly Task LoadTask;

    private Dictionary<string, FileItem> FileItems { get; set; } = new();

    private readonly SemaphoreSlim Lock = new(1, 1);

    private async Task Load()
    {
        this.FileItems = new();
        if (!File.Exists(FilePath))
        {
            return;
        }

        try
        {
            await Lock.WaitAsync();
            using var stream = File.OpenRead(this.FilePath);
            this.FileItems = await JsonSerializer.DeserializeAsync<Dictionary<string, FileItem>>(stream) ?? new();
        }
        catch (JsonException)
        {
            return;
        }
        finally
        {
            Lock.Release();
        }
    }

    public async Task<IList<FileItem>> GetItemsAsync()
    {
        await LoadTask;
        try
        {
            await Lock.WaitAsync();
            return this.FileItems.Values.ToArray();
        }
        finally
        {
            Lock.Release();
        }
    }

    public async Task<ulong> GetTotalSizeAsync()
    {
        var items = await this.GetItemsAsync();

        return (ulong)items.Sum(o => o.FileSize);
    }

    public async Task<FileItem?> GetAsync(string key)
    {
        await LoadTask;

        FileItem? item;
        try
        {
            await this.Lock.WaitAsync();

            if (!this.FileItems.TryGetValue(key, out item))
            {
                return null;
            }
        }
        finally
        {
            this.Lock.Release();
        }
        return item;
    }

    public async Task SetAsync(string key, FileItem item)
    {
        await LoadTask;

        try
        {
            await this.Lock.WaitAsync();
            this.FileItems[key] = item;

            using var stream = File.OpenWrite(this.FilePath);
            await JsonSerializer.SerializeAsync(stream, this.FileItems);
        }
        finally
        {
            this.Lock.Release();
        }
    }

    public async Task RemoveAsync(string hash)
    {
        await LoadTask;

        try
        {
            await this.Lock.WaitAsync();
            this.FileItems.Remove(hash);

            using var stream = File.OpenWrite(this.FilePath);
            await JsonSerializer.SerializeAsync(stream, this.FileItems);
        }
        finally
        {
            this.Lock.Release();
        }
    }
}
