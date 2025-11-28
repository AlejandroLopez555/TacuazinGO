using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TacuazinGO.Models;

namespace TacuazinGO.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Aquí agregaremos los DbSet para cada modelo
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        // ✅ NUEVO: DbSet para Archivos Financieros
        public DbSet<ArchivoFinanciero> ArchivosFinancieros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación Producto-Categoría
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ CORREGIDO: Configurar relaciones SIN referencia circular
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.ArchivoFinanciero)
                .WithMany()  // ← SIN la colección Ventas
                .HasForeignKey(v => v.ArchivoFinancieroId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.ArchivoFinanciero)
                .WithMany()  // ← SIN la colección Gastos
                .HasForeignKey(g => g.ArchivoFinancieroId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar precisión para decimales
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Venta>()
    .HasOne(v => v.ArchivoFinanciero)
    .WithMany(a => a.Ventas)
    .HasForeignKey(v => v.ArchivoFinancieroId)
    .OnDelete(DeleteBehavior.SetNull)
    .IsRequired(false);  // ← AÑADIR ESTO

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Gasto>()
    .HasOne(g => g.ArchivoFinanciero)
    .WithMany(a => a.Gastos)
    .HasForeignKey(g => g.ArchivoFinancieroId)
    .OnDelete(DeleteBehavior.SetNull)
    .IsRequired(false);  // ← AÑADIR ESTO

            modelBuilder.Entity<ArchivoFinanciero>()
                .Property(a => a.TotalVentasArchivadas)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ArchivoFinanciero>()
                .Property(a => a.TotalGastosArchivados)
                .HasColumnType("decimal(10,2)");
        }
    }
}