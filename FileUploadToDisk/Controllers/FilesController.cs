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

namespace FileUploadToDisk.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileUploadToDiskContext _context;

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

        private bool FileMetadataExists(int id)
        {
            return _context.FileMetadata.Any(e => e.Id == id);
        }
    }
}
