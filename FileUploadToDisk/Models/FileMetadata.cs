namespace FileUploadToDisk.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string? OriginalFilename { get; set; }

        // Generated/safe filename.
        public string? Filename { get; set; }
        public string? MimeType { get; set; }
        public DateTime UploadedDateTime { get; set; } = DateTime.Now;
        public long FileSize { get; set; }
    }
}
