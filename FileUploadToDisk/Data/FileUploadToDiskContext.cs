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
        // This is for testing, so (re)create database every time context is instantiated first time.
        private static bool _created = false;
        public FileUploadToDiskContext (DbContextOptions<FileUploadToDiskContext> options)
            : base(options)
        {
            if (!_created)
            {
                // Reinitialize database on every startup.
                _created = true;
                Database.EnsureDeleted();
                Database.EnsureCreated();
                SaveChanges();
            }
        }

        public DbSet<FileUploadToDisk.Models.FileMetadata> FileMetadata { get; set; }
    }
}
