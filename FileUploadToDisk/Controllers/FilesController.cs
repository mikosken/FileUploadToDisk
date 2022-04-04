#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileUploadToDisk.Data;
using FileUploadToDisk.Models;
using FileUploadToDisk.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SampleApp.Utilities;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using FileUploadToDisk.Utilities;

namespace FileUploadToDisk.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileUploadToDiskContext _context;
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly string[] _permittedExtensions = { ".txt", ".pdf" };
        private readonly long _fileSizeLimit = 4294967296;
        private readonly string _targetFilePath = @"C:\temptest\";

        public FilesController(FileUploadToDiskContext context)
        {
            _context = context;
        }

        // GET: Files
        public async Task<IActionResult> Index()
        {
            return View(await _context.FileMetadata.ToListAsync());
        }

        // GET: Files/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileMetadata = await _context.FileMetadata.FindAsync(id);
            if (fileMetadata == null)
            {
                return NotFound();
            }
            return View(fileMetadata);
        }

        // POST: Files/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalFilename,Filename,UploadedDateTime,FileSize")] FileMetadata fileMetadata)
        {
            if (id != fileMetadata.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fileMetadata);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileMetadataExists(fileMetadata.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fileMetadata);
        }

        // GET: Files/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileMetadata = await _context.FileMetadata
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileMetadata == null)
            {
                return NotFound();
            }

            return View(fileMetadata);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fileMetadata = await _context.FileMetadata.FindAsync(id);
            _context.FileMetadata.Remove(fileMetadata);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Files/
        public IActionResult Upload()
        {
            return View();
        }
        // POST: Files/ProcessUpload
        [HttpPost]
        [DisableFormValueModelBinding]

        [HttpGet]
        public async Task<IActionResult> DownloadFile([FromQuery] int fileId)
        {
            var md = await _context.FileMetadata.Where(f => f.Id == fileId).FirstOrDefaultAsync();

            if (md == null)
            {
                return NotFound();
            }

            var stream = new FileStream(Path.Combine(_targetFilePath, md.Filename) , FileMode.Open);
            return File(stream, md.MimeType, md.OriginalFilename);
        }


        // !!!!!
        // Can't use ValidateAntiForgeryToken as it reads the request body,
        // and then the data can't be read a second timeby the MultiPartReader, giving
        // an "IOException: Unexpected end of Stream, the content may have already been read by another component."
        // !!!!!
        //[ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967296)] // 4294967296 byte = 4 GB. 268435456 byte = 256 MB.
        [RequestSizeLimit(4294967296)] // 4 GB. Required to increase size limit for Kestrel.
        public async Task<IActionResult> ProcessUpload()
        {
            var uploadedFiles = await FileStreamingHelper.StreamFilesToDisk(Request, _targetFilePath);

            foreach (var meta in uploadedFiles)
            {
                // Add metadata object to context.
                _context.Add(meta);
            }

            await _context.SaveChangesAsync();

            // Perhaps return a nice summary of uploaded files?
            return RedirectToAction(nameof(Index));
        }
        private bool FileMetadataExists(int id)
        {
            return _context.FileMetadata.Any(e => e.Id == id);
        }
    }
}
