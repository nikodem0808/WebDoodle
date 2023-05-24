using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using WebDoodle.DataModels;
using WebDoodle.Migrations;
using Microsoft.AspNetCore.Server.IIS;
using WebDoodle.Services;

namespace WebDoodle.Controllers
{
    public class DrawController : Controller
    {

        AppDbContext dbctx;
        FileManager fileManager;

        public DrawController(AppDbContext _dbctx, FileManager _fileManager)
        {
            dbctx = _dbctx;
            fileManager = _fileManager;
        }

    }
}
