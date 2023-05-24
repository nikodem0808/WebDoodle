using WebDoodle.Migrations;

namespace WebDoodle.Services
{
    public class AppDbContextProvider
    {
        public AppDbContext dbctx { get; }

        public AppDbContextProvider(AppDbContext _dbctx)
        {
            dbctx = _dbctx;
        }
        
        public AppDbContext Get()
        {
            return dbctx;
        }
    }
}
