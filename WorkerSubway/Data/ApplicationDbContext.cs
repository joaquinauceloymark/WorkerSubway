using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WorkerSubwayPruebas.Models;

namespace WorkerSubwayPruebas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<clientes> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<clientes>()
                .HasKey(c => new { c.Cia, c.Codigo });

            modelBuilder.Entity<cli_clientes>().HasNoKey();

            modelBuilder.Entity<cli_clientes>()
           .Property(p => p.CLI_IDENTIFICACION)
           .HasColumnType("numeric(38, 0)");

            modelBuilder.Entity<cli_clientes>()
           .Property(p => p.CLI_PUNTOSDISPONIBLES)
           .HasColumnType("decimal(18, 2)");
        }
    }
}
