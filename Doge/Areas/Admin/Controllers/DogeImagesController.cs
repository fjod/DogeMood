using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Doge.Data;
using Doge.Models;
using System.IO;
using Doge.Utils;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using Serilog.Formatting.Compact.Reader;
using Serilog.Events;

namespace Doge.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DogeImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        IHostingEnvironment _env;
        public DogeImagesController(ApplicationDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        int totalPostOnPage = 10;
        // GET: Admin/DogeImages
        public async Task<IActionResult> Index(string sortOrder = "", int pageNumber = 1)
        {
            ViewData["PageIndex"] = pageNumber.ToString();
            PaginatedList<DogeImage> pages;
            if (sortOrder == "true")
            {
                ViewData["CurrentSort"] = "true";
                var dogesThumbnails =
                                        from img in _context.Images
                                        join post in _context.Posts
                                        on img.Post equals post
                                        where post.IsApproved == false

                                        let tempPost = new DogePost
                                        {
                                            DogeImage = img,
                                            UpVotes = post.UpVotes,
                                            IsApproved = post.IsApproved,
                                            Users = _context.UserPost.Where(up => up.DogePost == post).ToList()
                                        }
                                        select new DogeImage()
                                        {
                                            Id = img.Id,
                                            Pictogram = img.Pictogram,
                                            Post = tempPost
                                        };

                pages = await PaginatedList<DogeImage>.CreateAsync(dogesThumbnails, pageNumber, totalPostOnPage);

            }
            else
            {
                ViewData["CurrentSort"] = "";
                var dogesThumbnails =
                                      

                                       from img in _context.Images
                                       join post in _context.Posts
                                       on img.Post equals post


                                       let tempPost = new DogePost
                                       {
                                           DogeImage = img,
                                           UpVotes = post.UpVotes,
                                           IsApproved = post.IsApproved,
                                           Users = _context.UserPost.Where(up => up.DogePost == post).ToList()
                                       }
                                       select new DogeImage()
                                       {
                                           Id = img.Id,
                                           Pictogram = img.Pictogram,
                                           Post = tempPost
                                       };


                pages = await PaginatedList<DogeImage>.CreateAsync(dogesThumbnails, pageNumber, totalPostOnPage);
            }

            return View(pages);
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
            string sort = "";
            int index = 1;
            if (ViewData.ContainsKey("CurrentSort"))
                sort = ViewData["CurrentSort"].ToString();
            if (ViewData.ContainsKey("PageIndex"))
                index = int.Parse(ViewData["PageIndex"].ToString());

            return RedirectToAction(nameof(Index), new { sortOrder = sort, pageNumber = index });
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

            var dogePost = _context.Posts.Any(p => p.DogeImage == dogeImage);
            Console.WriteLine(dogePost);

            string sort = "";
            int index = 1;
            if (ViewData.ContainsKey("CurrentSort"))
                sort = ViewData["CurrentSort"].ToString();
            if(ViewData.ContainsKey("PageIndex"))
                index = int.Parse(ViewData["PageIndex"].ToString());

            return RedirectToAction(nameof(Index), new { sortOrder = sort, pageNumber = index });
        }

        public IActionResult IndexLogs()
        {
            var logPath = _env.WebRootPath + "\\logs";
            var logs = Directory.GetFiles(logPath).ToList();
           
            
            return View(logs);
        }

        public IActionResult BrowseLog(string logName)
        {
            List<LogEntry> logEntries = new List<LogEntry>();
            using (var clef = System.IO.File.OpenText(logName))
            {
                var reader = new LogEventReader(clef);
                LogEvent evt;
                while (reader.TryRead(out evt))
                    logEntries.Add(evt.Convert());
            }

               
            return View(logEntries);
        }

        public IActionResult DeleteLog(string logName)
        {
            System.IO.File.Delete(logName);
            return RedirectToAction(nameof(IndexLogs));
        }
    }

    public class LogEntry
    {
        [Key]
        public int MyProperty { get; set; } //scaffolding does not work without key
        public DateTimeOffset @t { get; set; }
        public string @mt { get; set; }
        public string SourceContext { get; set; }
    }
  
}
