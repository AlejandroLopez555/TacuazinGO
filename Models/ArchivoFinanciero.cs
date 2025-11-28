using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacuazinGO.Models
{
    public class ArchivoFinanciero
    {
        public int Id { get; set; }

        [Required]
        public string NombreArchivo { get; set; } = $"Archivo_{DateTime.Now:yyyyMMdd_HHmmss}";

        public DateTime FechaArchivado { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalVentasArchivadas { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalGastosArchivados { get; set; }

        public int CantidadVentasArchivadas { get; set; }
        public int CantidadGastosArchivados { get; set; }

        public string? Descripcion { get; set; }
        public string? UsuarioId { get; set; }

        // Relaciones
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}