using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic; 
using System;
namespace blazor.Components.Data
{
    public class Factura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaFactura { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        public string NombreCliente { get; set; } = string.Empty;
        public bool Archivada { get; set; } = false; 
        public List<ArticuloFactura> Articulos { get; set; } = new List<ArticuloFactura>();
        public decimal Total => Articulos.Sum(a => a.Subtotal);

        public class ArticuloFactura
        {
            public int Id { get; set; }
            public int FacturaId { get; set; }

            [Required(ErrorMessage = "La descripción es obligatoria.")]
            public string Descripcion { get; set; } = string.Empty;

            [Range(1, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
            public int Cantidad { get; set; } = 1;

            [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
            public decimal Precio { get; set; }
            public decimal Subtotal => Precio * Cantidad;
        }

        public class ReporteMensual
        {
            public int MesNumero { get; set; }
            public string NombreMes { get; set; } = string.Empty;
            public int CantidadFacturas { get; set; }
            public decimal TotalMes { get; set; }
        }
    }

    public class MétricaCliente
    {
        public string Cliente { get; set; } = string.Empty;
        public decimal TotalComprado { get; set; }
    }

    public class MétricaArticulo
    {
        public string Descripcion { get; set; } = string.Empty;
        public decimal TotalUnidades { get; set; }
        public decimal TotalIngreso { get; set; }
    }

    public class MétricaPico
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal TotalMes { get; set; }
    }

    public class MétricaDia
    {
        public string Dia { get; set; } = string.Empty;
        public decimal TotalVendido { get; set; }
    }
}