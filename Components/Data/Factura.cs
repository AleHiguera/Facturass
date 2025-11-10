using System.ComponentModel.DataAnnotations;

namespace blazor.Components.Data
{
    public class Factura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaFactura { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        public string NombreCliente { get; set; } = string.Empty;
        public List<ArticuloFactura> Articulos { get; set; } = new List<ArticuloFactura>();
        public decimal Total => Articulos.Sum(a => a.Precio);

        public class ArticuloFactura
        {
            public int Id { get; set; }
            public int FacturaId { get; set; }

            [Required(ErrorMessage = "La descripción es obligatoria.")]
            public string Descripcion { get; set; } = string.Empty;

            [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
            public decimal Precio { get; set; }
        }

    }
}
