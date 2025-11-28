namespace TacuazinGO.DTOs
{
    public class VentaDto
{
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
}