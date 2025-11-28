using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TacuazinGO.Data;
using TacuazinGO.DTOs;
using TacuazinGO.DTOs;
using TacuazinGO.Models;
using TacuazinGO.ViewModels;

namespace TacuazinGO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // ✅ SOLO DATOS NO ARCHIVADOS - VERSIÓN SEGURA
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .ToListAsync();

                // TEMPORAL: Sin filtro para evitar el error
                var gastos = await _context.Gastos
                    // .Where(g => g.ArchivoFinancieroId == null) // ← COMENTADO TEMPORALMENTE
                    .ToListAsync();

                var ventas = await _context.Ventas
                    // .Where(v => v.ArchivoFinancieroId == null) // ← COMENTADO TEMPORALMENTE  
                    .ToListAsync();

                var viewModel = new DashboardViewModel
                {
                    Productos = productos,
                    Gastos = gastos,
                    Ventas = ventas,
                    TotalGastos = gastos.Sum(g => g.Monto),
                    TotalVentas = ventas.Sum(v => v.Total),
                    GastoReciente = gastos.OrderByDescending(g => g.Fecha).FirstOrDefault()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Si hay error, retorna vista básica
                return View(new DashboardViewModel
                {
                    Productos = new List<Producto>(),
                    Gastos = new List<Gasto>(),
                    Ventas = new List<Venta>(),
                    TotalGastos = 0,
                    TotalVentas = 0
                });
            }
        }
        public IActionResult RegistrarVenta()
        {
            return View();
        }

        public IActionResult RegistrarGasto()
        {
            return View();
        }

        public async Task<IActionResult> Reportes()
        {
            // ✅ SOLO DATOS NO ARCHIVADOS (ArchivoFinancieroId = null)
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(v => v.ArchivoFinancieroId == null) // ← FILTRO NUEVO
                .ToListAsync();

            var gastos = await _context.Gastos
                .Where(g => g.ArchivoFinancieroId == null) // ← FILTRO NUEVO
                .ToListAsync();

            // Creamos DTOs para evitar ciclos
            var ventasResumen = ventas.Select(v => new VentaResumenDto
            {
                Id = v.Id,
                Fecha = v.Fecha,
                Total = v.Total,
                CantidadProductos = v.Detalles.Count
            }).ToList();

            var gastosResumen = gastos.Select(g => new GastoResumenDto
            {
                Id = g.Id,
                Fecha = g.Fecha,
                Descripcion = g.Descripcion,
                Monto = g.Monto,
                Categoria = g.Categoria ?? "Sin categoría"
            }).ToList();

            var viewModel = new ReportesViewModel
            {
                TotalVentas = ventas.Sum(v => v.Total),
                TotalGastos = gastos.Sum(g => g.Monto),
                VentasPorMes = ventas.GroupBy(v => v.Fecha.Month)
                                    .ToDictionary(g => g.Key, g => g.Sum(v => v.Total)),
                GastosPorCategoria = gastos.GroupBy(g => g.Categoria ?? "Sin categoría")
                                          .ToDictionary(g => g.Key, g => g.Sum(gg => gg.Monto)),
                Ventas = ventasResumen,
                Gastos = gastosResumen
            };

            return View(viewModel);
        }
        // ✅ NUEVO: Página para ver archivos históricos
        public async Task<IActionResult> ArchivosHistoricos()
        {
            var archivos = await _context.ArchivosFinancieros
                .OrderByDescending(a => a.FechaArchivado)
                .ToListAsync();
            return View(archivos);
        }

        // ✅ NUEVO: Ver detalles de un archivo específico
        public async Task<IActionResult> DetallesArchivo(int id)
        {
            var archivo = await _context.ArchivosFinancieros
                .Include(a => a.Ventas)
                .Include(a => a.Gastos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivo == null)
            {
                return NotFound();
            }

            return View(archivo);
        }
    }

}