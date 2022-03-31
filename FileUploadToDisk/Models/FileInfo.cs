namespace FileUploadToDisk.Models
{
    public class FileInfo
    {
        int Id { get; set; }
        string? OriginalFilename { get; set; }

        // Generated/safe filename.
        string? Filename { get; set; }

        DateTime UploadedDateTime { get; set; } = DateTime.Now;
        long FileSize { get; set; }
    }
}
