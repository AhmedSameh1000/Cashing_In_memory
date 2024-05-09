using Cashing.Models;
using Microsoft.EntityFrameworkCore;

namespace Cashing.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> products { get; set; }
    }
}