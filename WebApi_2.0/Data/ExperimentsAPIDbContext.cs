using Microsoft.EntityFrameworkCore;
using WebApi_2._0.Models;

namespace WebApi_2._0.Data
{
    public class ExperimentsAPIDbContext:DbContext
    {
        public ExperimentsAPIDbContext(DbContextOptions options):
            base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<FileModel> Files { get; set; }
        public DbSet<ValueModel> Values { get; set; }
        public DbSet<ResultModel> Results { get; set; }
    }
}
