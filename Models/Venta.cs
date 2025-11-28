using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacuazinGO.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [Display(Name = "Fecha Venta")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public string? UsuarioId { get; set; }

        // Relacion con detalles
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        public int? ArchivoFinancieroId { get; set; }
        public ArchivoFinanciero? ArchivoFinanciero { get; set; }
    }
}