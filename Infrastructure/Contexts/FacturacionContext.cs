using Facturación.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturación.Infrastructure.Contexts
{
    public class FacturacionContext:DbContext
    {
        public FacturacionContext(DbContextOptions<FacturacionContext> options)
        : base(options)
        {
        }

        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Factura>()
                .ToTable(nameof(Factura))
                .HasKey(f => f.Id);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
