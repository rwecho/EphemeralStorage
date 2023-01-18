using FileTtl.Controllers;

namespace FileTtl.BackgroundJobs;

public record DownloadInfo(FileItem FileItem, string FilePath);