using Microsoft.EntityFrameworkCore;
using WebDoodle.DataModels;

namespace WebDoodle.Migrations
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserData> UserDatas { get; set; }
        public DbSet<DrawingData> DrawingDatas { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            ;
        }
    }
}
