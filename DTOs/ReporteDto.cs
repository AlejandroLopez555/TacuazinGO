using TacuazinGO.Models;

namespace TacuazinGO.DTOs
{
    public class ReporteDto
    {
        public decimal TotalVentas { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance => TotalVentas - TotalGastos;
        public Dictionary<int, decimal> VentasPorMes { get; set; } = new();
        public Dictionary<string, decimal> GastosPorCategoria { get; set; } = new();
        public List<VentaResumenDto> Ventas { get; set; } = new();
        public List<GastoResumenDto> Gastos { get; set; } = new();
    }

    public class VentaResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class GastoResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; }
    }
}