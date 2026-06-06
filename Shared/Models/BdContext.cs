using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class BdContext(DbContextOptions<BdContext> options) : DbContext(options)
    {
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Laboratorios> Laboratorios { get; set; }
        public DbSet<Prestamos> Prestamos { get; set; }
        public DbSet<Encargados> Encargados { get; set; }
    }
}
