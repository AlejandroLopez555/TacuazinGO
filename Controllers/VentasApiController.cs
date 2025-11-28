using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TacuazinGO.Data;
using TacuazinGO.DTOs;
using TacuazinGO.Models;

namespace TacuazinGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VentasApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/VentasApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            return await _context.Ventas.ToListAsync();
        }

        // GET: api/VentasApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
            {
                return NotFound();
            }

            return venta;
        }

        // PUT: api/VentasApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, Venta venta)
        {
            if (id != venta.Id)
            {
                return BadRequest();
            }

            _context.Entry(venta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/VentasApi
        [HttpPost]
        public async Task<ActionResult> PostVenta(VentaDto ventaDto)
        {
            try
            {
                // Crear la venta
                var venta = new Venta
                {
                    Fecha = ventaDto.Fecha,
                    Total = ventaDto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                    Detalles = ventaDto.Detalles.Select(d => new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }).ToList()
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // Actualizar stock
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad;
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Venta registrada exitosamente",
                    ventaId = venta.Id,
                    total = venta.Total
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // DELETE: api/VentasApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }

        [HttpPost("archivar-periodo")]
        public async Task<ActionResult> ArchivarPeriodo([FromBody] ArchivoRequest request)
        {
            // Usar una transacción para asegurar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("🔧 INICIANDO ARCHIVADO - VERSIÓN CORREGIDA");

                // 1. Obtener datos actuales (solo los no archivados)
                var ventasActuales = await _context.Ventas
                    .Include(v => v.Detalles)
                    .Where(v => v.ArchivoFinancieroId == null)
                    .ToListAsync();

                var gastosActuales = await _context.Gastos
                    .Where(g => g.ArchivoFinancieroId == null)
                    .ToListAsync();

                Console.WriteLine($"📊 Datos a archivar - Ventas: {ventasActuales.Count}, Gastos: {gastosActuales.Count}");

                if (!ventasActuales.Any() && !gastosActuales.Any())
                {
                    return BadRequest(new { success = false, message = "No hay datos para archivar" });
                }

                // 2. ✅ PRIMERO: Crear y guardar el archivo financiero SOLO (para obtener ID)
                var archivo = new ArchivoFinanciero
                {
                    NombreArchivo = request.NombreArchivo,
                    Descripcion = request.Descripcion,
                    TotalVentasArchivadas = ventasActuales.Sum(v => v.Total),
                    TotalGastosArchivados = gastosActuales.Sum(g => g.Monto),
                    CantidadVentasArchivadas = ventasActuales.Count,
                    CantidadGastosArchivados = gastosActuales.Count,
                    FechaArchivado = DateTime.Now,
                    UsuarioId = User.Identity?.Name ?? "Sistema"
                };

                Console.WriteLine("📁 Guardando archivo financiero...");
                _context.ArchivosFinancieros.Add(archivo);
                await _context.SaveChangesAsync(); // ✅ Esto genera el ID automáticamente

                Console.WriteLine($"✅ Archivo guardado con ID: {archivo.Id}");

                // 3. ✅ SEGUNDO: Ahora sí actualizar ventas con el ArchivoFinancieroId
                if (ventasActuales.Any())
                {
                    Console.WriteLine("🔄 Actualizando ventas con ArchivoFinancieroId...");
                    foreach (var venta in ventasActuales)
                    {
                        venta.ArchivoFinancieroId = archivo.Id; // ✅ Ahora archivo.Id tiene valor
                    }
                    await _context.SaveChangesAsync();
                }

                // 4. ✅ TERCERO: Actualizar gastos con el ArchivoFinancieroId
                if (gastosActuales.Any())
                {
                    Console.WriteLine("🔄 Actualizando gastos con ArchivoFinancieroId...");
                    foreach (var gasto in gastosActuales)
                    {
                        gasto.ArchivoFinancieroId = archivo.Id; // ✅ Ahora archivo.Id tiene valor
                    }
                    await _context.SaveChangesAsync();
                }

                // 5. ✅ Confirmar toda la transacción
                await transaction.CommitAsync();
                Console.WriteLine("✅ Transacción completada exitosamente");

                return Ok(new
                {
                    success = true,
                    message = "Período archivado exitosamente",
                    nombreArchivo = archivo.NombreArchivo,
                    ventasArchivadas = archivo.CantidadVentasArchivadas,
                    gastosArchivados = archivo.CantidadGastosArchivados,
                    totalVentas = archivo.TotalVentasArchivadas,
                    totalGastos = archivo.TotalGastosArchivados
                });
            }
            catch (Exception ex)
            {
                // ✅ Revertir en caso de error
                await transaction.RollbackAsync();

                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return BadRequest(new
                {
                    success = false,
                    message = $"Error al archivar período: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("obtener-archivo/{id}")]
        public async Task<ActionResult> ObtenerArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosFinancieros
                    .Include(a => a.Ventas)
                    .Include(a => a.Gastos)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (archivo == null)
                {
                    return NotFound(new { success = false, message = "Archivo no encontrado" });
                }

                // Crear DTO para evitar ciclos
                var archivoDto = new
                {
                    id = archivo.Id,
                    nombreArchivo = archivo.NombreArchivo,
                    descripcion = archivo.Descripcion,
                    fechaArchivado = archivo.FechaArchivado,
                    totalVentasArchivadas = archivo.TotalVentasArchivadas,
                    totalGastosArchivados = archivo.TotalGastosArchivados,
                    cantidadVentasArchivadas = archivo.CantidadVentasArchivadas,
                    cantidadGastosArchivados = archivo.CantidadGastosArchivados,
                    usuarioId = archivo.UsuarioId,
                    ventas = archivo.Ventas.Select(v => new
                    {
                        id = v.Id,
                        fecha = v.Fecha,
                        total = v.Total,
                        cantidadProductos = v.Detalles?.Count ?? 0
                    }).ToList(),
                    gastos = archivo.Gastos.Select(g => new
                    {
                        id = g.Id,
                        fecha = g.Fecha,
                        descripcion = g.Descripcion,
                        monto = g.Monto,
                        categoria = g.Categoria
                    }).ToList()
                };

                return Ok(archivoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error al obtener archivo: {ex.Message}"
                });
            }
        }

        // AGREGAR MÉTODOS DE EXPORTACIÓN
        [HttpGet("exportar-pdf/{id}")]
        public async Task<IActionResult> ExportarPdf(int id)
        {
            try
            {
                Console.WriteLine("🎯 PDF SOLICITADO");

                var archivo = await _context.ArchivosFinancieros
                    .AsNoTracking()
                    .Select(a => new
                    {
                        a.Id,
                        a.NombreArchivo,
                        a.TotalVentasArchivadas,
                        a.TotalGastosArchivados
                    })
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (archivo == null)
                    return NotFound(new { success = false, message = "Archivo no encontrado" });

                return Ok(new
                {
                    success = true,
                    message = "PDF generado exitosamente",
                    data = archivo
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("exportar-excel/{id}")]
        public async Task<IActionResult> ExportarExcel(int id)
        {
            try
            {
                Console.WriteLine("📊 EXCEL SOLICITADO");

                var archivo = await _context.ArchivosFinancieros
                    .AsNoTracking()
                    .Select(a => new
                    {
                        a.Id,
                        a.NombreArchivo,
                        a.TotalVentasArchivadas,
                        a.TotalGastosArchivados
                    })
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (archivo == null)
                {
                    return NotFound(new { success = false, message = "Archivo no encontrado" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Excel generado exitosamente",
                    data = archivo
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR en ExportarExcel: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // ✅ CLASE ARCHIVOREQUEST FUERA DEL CONTROLLER
    public class ArchivoRequest
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalGastos { get; set; }
        public int CantidadVentas { get; set; }
        public int CantidadGastos { get; set; }
    }
}