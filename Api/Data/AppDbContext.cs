using Microsoft.EntityFrameworkCore;
using Shared.Models;
namespace Api.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Laboratorios> Laboratorios { get; set; }
        public DbSet<Peticiones> Peticiones { get; set; }
        public DbSet<Respuesta> Respuestas { get; set; }
        public DbSet<Encargados> Encargados { get; set; }
    }
}
