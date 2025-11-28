using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacuazinGO.Models
{
    public class Gasto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Display(Name = "Fecha del Gasto")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? Categoria { get; set; }

        public string? UsuarioId { get; set; }
        public int? ArchivoFinancieroId { get; set; }
        public ArchivoFinanciero? ArchivoFinanciero { get; set; }
    }
}