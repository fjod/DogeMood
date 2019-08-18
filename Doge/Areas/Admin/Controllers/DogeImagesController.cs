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
using Microsoft.AspNetCore.Authorization;

namespace Doge.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DogeImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        readonly IHostingEnvironment _env;
        public DogeImagesController(ApplicationDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public readonly int totalPostOnPage = 10;
        // GET: Admin/DogeImages
        [Authorize]
        public async Task<IActionResult> Index(string sortOrder = "", int pageNumber = 1)
        {
            ViewData["PageIndex"] = pageNumber.ToString();
            if (!ViewData.ContainsKey("CurrentSort"))
                ViewData.Add("CurrentSort", sortOrder);
            else
            ViewData["CurrentSort"] = sortOrder;



            PaginatedList <DogeSmallImage> pages;
            if (sortOrder == "UnApprovedOnly")
            {           
                var dogesThumbnails = _context.SmallImages.
                    Include(im => im.Post).
                    ThenInclude(post => post.Users).Where(p => p.Post.IsApproved == false).AsQueryable();

                pages = await PaginatedList<DogeSmallImage>.CreateAsync(dogesThumbnails, pageNumber, totalPostOnPage);

            }

            else
            {
            
                var dogesThumbnails = _context.SmallImages.
                    Include(im => im.Post).
                    ThenInclude(post => post.Users).AsQueryable();

                pages = await PaginatedList<DogeSmallImage>.CreateAsync(dogesThumbnails, pageNumber, totalPostOnPage);

            }

            if (!TempData.ContainsKey("PageIndex"))
                TempData.Add("PageIndex", pageNumber);
            else TempData["PageIndex"] = pageNumber;

            if (!TempData.ContainsKey("CurrentSort"))
                TempData.Add("CurrentSort", pageNumber);
            else TempData["CurrentSort"] = sortOrder;

            return View(pages);
        }

        // GET: Admin/DogeImages/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogeImage = await _context.BigImages.Include(im => im.DogeSmallImage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dogeImage == null)
            {
                return NotFound();
            }

            return View(dogeImage);
        }
        [Authorize]
        public async Task<IActionResult> Approve(int? id)
        {
            //id is Image id          

            var p3 = (from im in _context.BigImages
                     where im.Id == id
                     join p in _context.Posts on im.Id equals p.DogeImage.Id
                     select p).FirstOrDefault();

            p3.IsApproved = true;
            await _context.SaveChangesAsync();

            var retParams = GetCurrentViewDataParams();

            return RedirectToAction(nameof(Index), new { sortOrder = retParams.Item1, pageNumber = retParams.Item1 });
        }

        Tuple<string,int> GetCurrentViewDataParams()
        {
            string sort = "";
            int index = 1;
            if (TempData.ContainsKey("CurrentSort"))
                sort = TempData.Peek("CurrentSort").ToString();
            if (TempData.ContainsKey("PageIndex"))
                index = int.Parse(TempData.Peek("PageIndex").ToString());
            return new Tuple<string, int>(sort, index);
        }

        // GET: Admin/DogeImages/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogeImage = await _context.BigImages.Include(im => im.DogeSmallImage)
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
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bdogeImage = await _context.SmallImages.FindAsync(id);
            _context.SmallImages.Remove(bdogeImage);
            var  sdogeImage = await _context.BigImages.FindAsync(id);
            _context.BigImages.Remove(sdogeImage);            
          
            await _context.SaveChangesAsync();


            var retParams = GetCurrentViewDataParams();

            return RedirectToAction(nameof(Index), new { sortOrder = retParams.Item1, pageNumber = retParams.Item1 });

        }

        #region ------------------------------------logs
        [Authorize]
        public IActionResult IndexLogs()
        {
            var logPath = _env.WebRootPath + "\\logs";
            var logs = Directory.GetFiles(logPath).ToList();


            return View(logs);
        }
        [Authorize]
        public IActionResult BrowseLog(string logName)
        {
            List<LogEntry> logEntries = new List<LogEntry>();
            if (System.IO.File.Exists(logName))
            {
                Stream stream = System.IO.File.Open(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader streamReader = new StreamReader(stream);
                string str = streamReader.ReadToEnd();

                //using (var clef = System.IO.File.OpenText(logName))
                using (var clef = new StringReader(str))
                {
                    
                    var reader = new LogEventReader(clef);
                    while (reader.TryRead(out LogEvent evt))
                        logEntries.Add(evt.Convert());
                    reader.Dispose();
                }
            }
            else
            {
                logEntries.Add(new LogEntry
                {
                    mt = "",
                    MyProperty = 1,
                    SourceContext = "no such log found",
                    t = DateTime.Now
                });
            }


            return View(logEntries);
        }
        [Authorize]
        public IActionResult DeleteLog(string logName)
        {
            if (System.IO.File.Exists(logName))
                System.IO.File.Delete(logName);
            return RedirectToAction(nameof(IndexLogs));
        }
        #endregion
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]

    public class LogEntry
    {
        [Key]
        public int MyProperty { get; set; } //scaffolding does not work without key

        public DateTimeOffset @t { get; set; }
        public string @mt { get; set; }
        public string SourceContext { get; set; }
    }

}
