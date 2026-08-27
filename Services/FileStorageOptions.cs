namespace HRSystem.API.Services;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "PrivateStorage";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } =
        [".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".txt"];
    public string[] AllowedMimeTypes { get; set; } =
        ["application/pdf", "image/png", "image/jpeg", "application/msword",
         "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
         "application/vnd.ms-excel",
         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
         "text/plain"];
}
