using System.ComponentModel.DataAnnotations;

namespace TacuazinGO.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [Display(Name = "Nombre de Categoría")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relacion con Productos
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}