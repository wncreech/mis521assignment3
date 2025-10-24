using Betterboxd.Models;
using Microsoft.EntityFrameworkCore;

namespace Betterboxd.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Actor> Actor { get; set; }
    }
}