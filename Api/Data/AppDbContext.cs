using Microsoft.EntityFrameworkCore;
using Api.Models;
namespace Api.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Usuarios> usuarios { get; set; }
     
        public DbSet<Laboratorios> laboratorios { get; set; }
        public DbSet<Prestamos> prestamos { get; set; }
        public DbSet<Encargados> encargados { get; set; }
    }
}
