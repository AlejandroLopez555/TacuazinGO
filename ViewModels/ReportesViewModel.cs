using TacuazinGO.DTOs;

namespace TacuazinGO.ViewModels
{
    public class ReportesViewModel
    {
        public decimal TotalVentas { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance => TotalVentas - TotalGastos;
        public Dictionary<int, decimal> VentasPorMes { get; set; } = new();
        public Dictionary<string, decimal> GastosPorCategoria { get; set; } = new();
        public List<VentaResumenDto> Ventas { get; set; } = new();
        public List<GastoResumenDto> Gastos { get; set; } = new();
    }
}