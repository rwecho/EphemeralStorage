namespace FileTtl.Controllers;

public record FileItem(string Hash, string FileName, string ContentType, string Extensions, long FileSize, DateTime LastModified);
