using TacuazinGO.Models;

namespace TacuazinGO.ViewModels
{
    public class DashboardViewModel
    {
        public List<Producto> Productos { get; set; } = new();
        public List<Gasto> Gastos { get; set; } = new();
        public List<Venta> Ventas { get; set; } = new();
        public decimal TotalGastos { get; set; }
        public decimal TotalVentas { get; set; }
        public Gasto? GastoReciente { get; set; }
    }
}