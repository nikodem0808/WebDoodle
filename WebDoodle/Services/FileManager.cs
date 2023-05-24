using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Client;
using System.IO;
using System.Text;
using WebDoodle.DataModels;

namespace WebDoodle.Services
{
    public class FileManager
    {

        IWebHostEnvironment webEnv;

        public FileManager(IWebHostEnvironment _webEnv)
        {
            webEnv = _webEnv;
        }

        // saves drawing to a file for the given user
        // returns a path that can later be interpreted by FileManager
        public string SaveDrawing(int userId, int drawingId, string drawingData)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            CreateUserDrawingFile(userId);
            string filePath = $"{flp.GetFileInfo("/Data/Drawings").PhysicalPath}/{userId}/{drawingId}.json";
            FileStream fl = ( File.Exists(filePath) ? File.Open(filePath, FileMode.Truncate) : File.Create(filePath) );
            fl.Write(Encoding.ASCII.GetBytes(drawingData));
            fl.Close();
            return filePath;
        }

        public bool DeleteDrawing(int userId, int drawingId)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            string filePath = $"{flp.GetFileInfo("/Data/Drawings").PhysicalPath}/{userId}/{drawingId}.json";
            if (!File.Exists(filePath))
            {
                return false;
            }
            File.Delete(filePath);
            return true;
        }

        public string GetFileContents(string path)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            FileStream fl = File.OpenRead(path);
            string contents = (new StreamReader(fl)).ReadToEnd();
            fl.Close();
            return contents;
        }

        public async Task<string> GetFileContentsAsync(string path)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            FileStream fl = File.OpenRead(path);
            string contents = await (new StreamReader(fl)).ReadToEndAsync();
            fl.Close();
            return contents;
        }

        public void CreateUserDrawingFile(int userId)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            string filePath = $"{flp.GetFileInfo("/Data/Drawings").PhysicalPath}/{userId}";
            if (Directory.Exists(filePath))
            {
                return;
            }
            Directory.CreateDirectory(filePath);
        }

        public void DeleteUserDrawingFile(int userId)
        {
            IFileProvider flp = webEnv.ContentRootFileProvider;
            string filePath = $"{flp.GetFileInfo("/Data/Drawings").PhysicalPath}/{userId}";
            if (!Directory.Exists(filePath))
            {
                return;
            }
            foreach (string drawingFilePath in Directory.GetFiles(filePath))
            {
                File.Delete(drawingFilePath);
            }
            Directory.Delete(filePath);
        }

    }
}
