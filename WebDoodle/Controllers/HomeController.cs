using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WebDoodle.DataModels;
using WebDoodle.Migrations;
using WebDoodle.Models;
using WebDoodle.Services;

namespace WebDoodle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public AppDbContext dbctx;
        public static FileManager fileManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext _dbctx, FileManager _fileManager)
        {
            _logger = logger;
            dbctx = _dbctx;
            fileManager = _fileManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Draw()
        {
            try
            {
                DrawingData drawingData = null;
                int id = (int)HttpContext.Session.GetInt32("EditID");
                int uid = (int)HttpContext.Session.GetInt32("UID");
                DrawingData[] queryResults = (
                    from entry in dbctx.DrawingDatas
                    where entry.id == id
                       && entry.uid == uid
                    select entry
                ).ToArray();
                if (queryResults.Length < 1)
                {
                    return View(model: null);
                }
                drawingData = queryResults[0];
                HttpContext.Session.Remove("EditID");
                return View(model: drawingData);
            }
            catch (Exception ex) { }
            return View(model: null);
        }

        public IActionResult Gallery()
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