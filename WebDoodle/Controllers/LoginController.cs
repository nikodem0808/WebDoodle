using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using WebDoodle.Migrations;
using System.Linq;
using WebDoodle.DataModels;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Primitives;
using System.ComponentModel;
using System.Text;
using Microsoft.Identity.Client;
using NuGet.Packaging;
using WebDoodle.Services;
using Microsoft.CodeAnalysis.Scripting;

namespace WebDoodle.Controllers
{
    public class LoginController : Controller
    {
        public MD5 Hash;
        public AppDbContext dbctx;
        public FileManager fileManager;

        public LoginController(AppDbContext dbContext, FileManager _fileManager)
        {
            Hash = MD5.Create("MD5");
            dbctx = dbContext;
            fileManager = _fileManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            if ((HttpContext.Session.GetString("LoggedIn") ?? "False") == "True") { return View(); }
            string username = Request.Form["Username"].ToString() ?? "";
            string password = Encoding.ASCII.GetString(Hash.ComputeHash(Encoding.Unicode.GetBytes(Request.Form["Password"].ToString() ?? ""))) ?? "";
            List<UserData> queryResults = (
                from userData in dbctx.UserDatas
                where userData.Username == username
                select userData
            ).ToList();
            if ( queryResults.Count == 0 ) { return View("Index"); }
            if ( queryResults.Count > 1 ) { Console.Error.WriteLine($"Error: found more than one user with the username {username}"); }
            UserData user = queryResults[0];
            if (user.Password != password) { return View("Index"); };
            HttpContext.Session.SetString("LoggedIn", "True");
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("password", user.Password);
            HttpContext.Session.SetInt32("UID", user.uid);
            Response.Redirect("/Home");
            return View("Index");
        }

        public IActionResult Create()
        {
            if ((HttpContext.Session.GetString("LoggedIn") ?? "False") == "True") { return View(); }
            string username = Request.Form["Username"].ToString() ?? "";
            string password = Encoding.ASCII.GetString(Hash.ComputeHash(Encoding.Unicode.GetBytes(Request.Form["Password"].ToString() ?? ""))) ?? "";
            List<UserData> queryResults = (
                from userData in dbctx.UserDatas
                where userData.Username == username
                select userData
            ).ToList();
            if (queryResults.Count == 1) { return View(); }
            if (queryResults.Count > 1) { Console.Error.WriteLine($"Error: found more than one user with the username {username}"); }
            UserData user = new UserData();
            user.Username = username;
            user.Password = password;
            dbctx.UserDatas.Add(user);
            dbctx.SaveChanges();
            user = (from userData in dbctx.UserDatas where (userData.Username == username) select userData).ToArray()[0];
            fileManager.CreateUserDrawingFile(user.uid);
            HttpContext.Session.SetString("LoggedIn", "True");
            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetString("password", user.Password);
            HttpContext.Session.SetInt32("UID", user.uid);
            Response.Redirect("/Home");
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        public IActionResult Manage()
        {
            if ((HttpContext.Session.GetString("LoggedIn") ?? "False") == "False")
            {
                return View("Index");
            }
            return View();
        }

        public IActionResult Delete()
        {
            if ((HttpContext.Session.GetString("LoggedIn") ?? "False") == "False")
            {
                return View("Index");
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
