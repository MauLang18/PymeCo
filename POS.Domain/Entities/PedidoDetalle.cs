namespace POS.Domain.Entities
{
    public class PedidoDetalle
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Descuento { get; set; }
        public decimal ImpuestoPorc { get; set; }
        public decimal TotalLinea { get; set; }

        // Navegación
        public Pedido? Pedido { get; set; }
        public Product? Producto { get; set; }
    }
}
