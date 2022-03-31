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

namespace FileUploadToDisk.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileUploadToDiskContext _context;
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly string[] _permittedExtensions = { ".txt", ".pdf" };
        private readonly long _fileSizeLimit = 268435456;
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

        // GET: Files/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Files/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Files/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OriginalFilename,Filename,UploadedDateTime,FileSize")] FileMetadata fileMetadata)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fileMetadata);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fileMetadata);
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

        // !!!!!
        // Can't use ValidateAntiForgeryToken as it reads the request body,
        // and then the data can't be read a second timeby the MultiPartReader, giving
        // an "IOException: Unexpected end of Stream, the content may have already been read by another component."
        // !!!!!
        //[ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        public async Task<IActionResult> ProcessUpload()
        {
            // A large part of this code comes from:
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-6.0#upload-large-files-with-streaming
            //
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed because it wasn't Multipart Content.");
                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        //ModelState.AddModelError("File",
                        //    $"The request couldn't be processed (Error 2).");
                        //// Log error

                        //return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);

                            // Log that file has been written here.
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            // Perhaps return a nice summary of uploaded files?
            return RedirectToAction(nameof(Index));
        }
        private bool FileMetadataExists(int id)
        {
            return _context.FileMetadata.Any(e => e.Id == id);
        }
    }
}
