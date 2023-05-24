using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using WebDoodle.DataModels;
using WebDoodle.Migrations;
using WebDoodle.Services;

namespace WebDoodle.Endpoints
{
    public class Endpoints
    {
        WebApplication app;

        public Endpoints(WebApplication _app)
        {
            app = _app;
        }

        public async Task get_drawing(HttpContext ctx)
        {
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False" || ctx.Request.Headers["ID"] == StringValues.Empty)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.WriteAsync("");
                return;
            }
            int uid = ctx.Session.GetInt32("UID") ?? -1;
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            string[] queryResult = (
                from entry in dbctx.DrawingDatas
                where entry.id == int.Parse(ctx.Request.Headers["ID"])
                   && entry.uid == uid
                select entry.data
            ).ToArray();
            if (queryResult.Length < 1)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.WriteAsync("");
                return;
            }
            string path = queryResult.First();
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            await ctx.Response.WriteAsync(app.Services.GetService<FileManager>().GetFileContents(path));
        }

        public async Task get_drawing_page(HttpContext ctx)
        {
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False" || ctx.Request.Headers["page"] == StringValues.Empty)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.WriteAsync("");
                return;
            }
            int page = -1;
            const int pageSize = 6;
            try
            {
                page = int.Parse(ctx.Request.Headers["page"]);
            }
            catch (FormatException e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.WriteAsync("");
                return;
            }
            int uid = ctx.Session.GetInt32("UID") ?? -1;
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            StringBuilder response = new StringBuilder("[");
            DrawingData[] drawings = (
                from entry in dbctx.DrawingDatas
                where entry.uid == uid
                select entry
            ).Skip(pageSize * page).Take(pageSize).ToArray();
            if (drawings.Length > 0)
            {
                FileManager fileManager = app.Services.GetService<FileManager>();
                response.Append("{\"id\":");
                response.Append(drawings.First().id);
                response.Append(", \"data\":");
                response.Append(fileManager.GetFileContents(drawings.First().data));
                response.Append("}");
                foreach (var drawing in drawings.AsSpan(1).ToArray())
                {
                    response.Append(",{\"id\":");
                    response.Append(drawing.id);
                    response.Append(", \"data\":");
                    response.Append(fileManager.GetFileContents(drawing.data));
                    response.Append("}");
                }
            }
            response.Append("]");
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            var _response = response.ToString();
            await ctx.Response.WriteAsync(_response);
            return;
        }

        public async Task save_drawing(HttpContext ctx)
        {
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            FileManager fileManager = ctx.RequestServices.GetService<FileManager>();
            //
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False")
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            string body = await (new StreamReader(ctx.Request.Body).ReadToEndAsync());
            if (body.Length == 0)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            //
            int uid = (int)ctx.Session.GetInt32("UID");
            if ((ctx.Request.Headers["ID"].ToString() ?? "") == "")
            {
                DrawingData drawingData = new DrawingData();
                drawingData.uid = uid;
                drawingData.data = "";
                dbctx.DrawingDatas.Add(drawingData);
                dbctx.SaveChanges();
                int did = drawingData.id;
                //
                drawingData.data = fileManager.SaveDrawing(uid, did, body);
                dbctx.DrawingDatas.Update(drawingData);
                dbctx.SaveChanges();
            }
            else
            {
                DrawingData? drawingData = null;
                //
                try
                {
                    drawingData = (
                        from entry in dbctx.DrawingDatas
                        where entry.id == int.Parse(ctx.Request.Headers["ID"])
                           && entry.uid == uid
                        select entry
                    ).ToArray()[0];
                }
                catch (Exception e)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                int did = drawingData.id;
                //
                drawingData.data = fileManager.SaveDrawing(uid, did, body);
            }
            //
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        public async Task delete_drawing(HttpContext ctx)
        {
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            FileManager fileManager = ctx.RequestServices.GetService<FileManager>();
            //
            if (
                (ctx.Session.GetString("LoggedIn") ?? "False") == "False"
                || (ctx.Request.Headers["ID"] == StringValues.Empty)
            )
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            //
            int uid = (int)ctx.Session.GetInt32("UID");
            int did = int.Parse(ctx.Request.Headers["ID"].ToString());
            if (fileManager.DeleteDrawing(uid, did))
            {
                DrawingData drawingData = (
                    from entry in dbctx.DrawingDatas
                    where entry.uid == uid
                       && entry.id == did
                    select entry
                ).ToArray().First();
                dbctx.DrawingDatas.Remove(drawingData);
                dbctx.SaveChanges();
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            ctx.Response.StatusCode = (int)HttpStatusCode.Gone;
            return;
        }

        public async Task put_for_edit(HttpContext ctx)
        {
            try
            {
                int did = int.Parse(ctx.Request.Headers["ID"]);
                ctx.Session.SetInt32("EditID", did);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            return;
        }

        public async Task delete_account(HttpContext ctx)
        {
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False"
                || ctx.Request.Headers["password"] == StringValues.Empty)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            int uid = (int)ctx.Session.GetInt32("UID");
            string GivenPassword = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(ctx.Request.Headers["password"])));
            FileManager fileManager = app.Services.GetService<FileManager>();
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            UserData userData = (
                from entry in dbctx.UserDatas
                where entry.uid == uid
                select entry
            ).ToArray().First();
            if (GivenPassword != userData.Password)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            DrawingData[] drawingDatas = (
                from entry in dbctx.DrawingDatas
                where entry.uid == uid
                select entry
            ).ToArray();
            dbctx.UserDatas.Remove(userData);
            dbctx.DrawingDatas.RemoveRange(drawingDatas);
            dbctx.SaveChanges();
            fileManager.DeleteUserDrawingFile(uid);
            ctx.Session.Clear();
            //
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            return;
        }

        public async Task edit_username(HttpContext ctx)
        {
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False"
                || ctx.Request.Headers["field"] == StringValues.Empty
                || ctx.Request.Headers["password"] == StringValues.Empty)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            int uid = (int)ctx.Session.GetInt32("UID");
            string NewUsername = ctx.Request.Headers["field"];
            string GivenPassword = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(ctx.Request.Headers["password"])));
            if (GivenPassword != ctx.Session.GetString("password"))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            try
            {
                UserData userData = (
                    from entry in dbctx.UserDatas
                    where entry.uid == uid
                       && entry.Password == GivenPassword
                    select entry
                ).ToArray().First();
                userData.Username = NewUsername;
                dbctx.UserDatas.Update(userData);
                dbctx.SaveChanges();
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            ctx.Session.SetString("usernme", NewUsername);
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            return;
        }

        public async Task edit_password(HttpContext ctx)
        {
            if ((ctx.Session.GetString("LoggedIn") ?? "False") == "False"
                || ctx.Request.Headers["field"] == StringValues.Empty
                || ctx.Request.Headers["password"] == StringValues.Empty)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            int uid = (int)ctx.Session.GetInt32("UID");
            string NewPassword = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(ctx.Request.Headers["field"])));
            string GivenPassword = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(ctx.Request.Headers["password"])));
            if (GivenPassword != ctx.Session.GetString("password"))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            AppDbContext dbctx = ctx.RequestServices.GetRequiredService<AppDbContext>();
            try
            {
                UserData userData = (
                    from entry in dbctx.UserDatas
                    where entry.uid == uid
                       && entry.Password == GivenPassword
                    select entry
                ).ToArray().First();
                userData.Password = NewPassword;
                dbctx.UserDatas.Update(userData);
                dbctx.SaveChanges();
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            ctx.Session.SetString("password", NewPassword);
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            return;
        }
    }
}
