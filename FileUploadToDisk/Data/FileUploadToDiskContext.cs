#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FileUploadToDisk.Models;

namespace FileUploadToDisk.Data
{
    public class FileUploadToDiskContext : DbContext
    {
        public FileUploadToDiskContext (DbContextOptions<FileUploadToDiskContext> options)
            : base(options)
        {
        }

        public DbSet<FileUploadToDisk.Models.FileMetadata> FileMetadata { get; set; }
    }
}
