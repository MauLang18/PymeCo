using System;
using System.Collections.Generic;

namespace POS.Domain.Entities
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }

        // Estados válidos en BD: Pendiente, Pagado, Enviado, Cancelado
        public string Estado { get; set; } = "Pendiente";

        // Navegación
        public Client? Cliente { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}
