using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Doge.Models;
using Doge.Data;
using Doge.Areas.User.Models;

namespace Doge.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        ApplicationDbContext _db { get; set; }
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<DogePostForUser> lt = new List<DogePostForUser>();
            lt.Add(new DogePostForUser
            {
                WasFavorited = true,
                WasLiked = true
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = false,
                WasLiked = true
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = true,
                WasLiked = false
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = false,
                WasLiked = false
            });
            return View(lt);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
