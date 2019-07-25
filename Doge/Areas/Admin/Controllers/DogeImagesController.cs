using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Doge.Data;
using Doge.Models;

namespace Doge.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DogeImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DogeImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DogeImages
        public async Task<IActionResult> Index(string ShowUnApprovedOnly = "")
        {
            if (ShowUnApprovedOnly == "true")
            {
                var dogesThumbnails = from img in _context.Images
                                      join post in _context.Posts                                      
                                      on  img.Post equals post
                                      where post.IsApproved == false
                                      select new DogeImage()
                                      {
                                          Id = img.Id,
                                          Pictogram = img.Pictogram
                                      };
                return View(await dogesThumbnails.ToListAsync());
            }
            else
            {
                var dogesThumbnails = from img in _context.Images
                                      select new DogeImage()
                                      {
                                          Id = img.Id,
                                          Pictogram = img.Pictogram
                                      };
                return View(await dogesThumbnails.ToListAsync());
            }
        }

        // GET: Admin/DogeImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogeImage = await _context.Images
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dogeImage == null)
            {
                return NotFound();
            }

            return View(dogeImage);
        }

        public async Task<IActionResult> Approve(int? id)
        {
            //id is Image id
            var post = (from p in _context.Posts
                       where p.Id == 
                          (from im in _context.Images where im.Id == id select im).FirstOrDefault().Id
                       select p).FirstOrDefault();

            post.IsApproved = true;
            await _context.SaveChangesAsync();

            //redirect to same page with only favorite posts displayed
            return RedirectToAction("Index","true");
        }

        // GET: Admin/DogeImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogeImage = await _context.Images
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dogeImage == null)
            {
                return NotFound();
            }

            return View(dogeImage);
        }

        // POST: Admin/DogeImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dogeImage = await _context.Images.FindAsync(id);
            _context.Images.Remove(dogeImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

      
    }
}
