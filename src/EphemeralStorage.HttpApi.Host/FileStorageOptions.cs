namespace EphemeralStorage.BackgroundJobs;

public class FileStorageOptions
{
    public int ValidHours { get; set; } = 24;

    public int MaxStorageLimit { get; set; } = 0; //GB
}