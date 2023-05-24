using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using WebDoodle.DataModels;
using WebDoodle.Endpoints;
using WebDoodle.Migrations;
using WebDoodle.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddConnections();
builder.Services.AddSession();
builder.Services.AddDbContext<AppDbContext>(
        options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));
builder.Services.AddTransient<FileManager>();
builder.Services.AddTransient<AppDbContextProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();

Endpoints UserEndpoints = new Endpoints(app);
app.Map("/get_drawing", UserEndpoints.get_drawing);
app.Map("/get_drawing_page", UserEndpoints.get_drawing_page);
app.Map("/save_drawing", UserEndpoints.save_drawing);
app.Map("/delete_drawing", UserEndpoints.delete_drawing);
app.Map("/put_for_edit", UserEndpoints.put_for_edit);
app.Map("/delete_account", UserEndpoints.delete_account);
app.Map("/edit_username", UserEndpoints.edit_username);
app.Map("/edit_password", UserEndpoints.edit_password);




app.Run();
