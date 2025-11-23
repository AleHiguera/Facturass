using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using blazor.Components.Data;
using blazor.Components.Servicios;

namespace blazor.Components.Servicios
{
    public class ServicioDashboard
    {
        private readonly ServicioControlador _controlador;

        public ServicioDashboard(ServicioControlador controlador)
        {
            _controlador = controlador;
        }

        public async Task<decimal> ObtenerIngresosTotalesAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();
            return facturas.Sum(f => f.Total);
        }
        public async Task<int> ObtenerTotalFacturasAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();
            return facturas.Count();
        }

        public async Task<MétricaPico?> ObtenerPicoIngresosAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();

            var resultado = facturas
                .GroupBy(f => new { f.FechaFactura.Year, f.FechaFactura.Month })
                .Select(g => new MétricaPico
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NombreMes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM", new CultureInfo("es-MX")),
                    TotalMes = g.Sum(f => f.Total)
                })
                .OrderByDescending(r => r.TotalMes)
                .FirstOrDefault();

            return resultado;
        }
        public async Task<MétricaArticulo?> ObtenerArticuloMasVendidoPorCantidadAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();

            var resultado = facturas
                .SelectMany(f => f.Articulos)
                .GroupBy(a => a.Descripcion)
                .Select(g => new
                {
                    Descripcion = g.Key,
                    TotalUnidades = g.Sum(a => a.Cantidad)
                })
                .OrderByDescending(r => r.TotalUnidades)
                .FirstOrDefault();

            if (resultado == null) return null;
            return new MétricaArticulo
            {
                Descripcion = resultado.Descripcion,
                TotalUnidades = resultado.TotalUnidades,
                TotalIngreso = 0
            };
        }

        public async Task<decimal> ObtenerTicketPromedioAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();

            if (!facturas.Any()) return 0m;

            return facturas.Average(f => f.Total);
        }

        public async Task<IEnumerable<MétricaCliente>> ObtenerTopClientesAsync(int top = 3)
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();
            return facturas
                .GroupBy(f => f.NombreCliente)
                .Select(g => new MétricaCliente
                {
                    Cliente = g.Key,
                    TotalComprado = g.Sum(f => f.Total)
                })
                .OrderByDescending(r => r.TotalComprado)
                .Take(top)
                .ToList();
        }
        public async Task<int> ObtenerTotalClientesUnicosAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();
            return facturas
                .Select(f => f.NombreCliente)
                .Distinct()
                .Count();
        }
        public async Task<MétricaArticulo?> ObtenerArticuloMasRentableAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();

            var resultado = facturas
                .SelectMany(f => f.Articulos)
                .GroupBy(a => a.Descripcion)
                .Select(g => new
                {
                    Descripcion = g.Key,
                    TotalIngreso = g.Sum(a => a.Cantidad * a.Precio)
                })
                .OrderByDescending(r => r.TotalIngreso)
                .FirstOrDefault();

            if (resultado == null) return null;

            return new MétricaArticulo
            {
                Descripcion = resultado.Descripcion,
                TotalUnidades = 0, 
                TotalIngreso = resultado.TotalIngreso
            };
        }
        public async Task<IEnumerable<MétricaDia>> ObtenerVentasPorDiaDeLaSemanaAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();

            var resultado = facturas
                .GroupBy(f => f.FechaFactura.DayOfWeek)
                .Select(g => new MétricaDia
                {
                    Dia = g.Key.ToString(),
                    TotalVendido = g.Sum(f => f.Total)
                })
                .OrderBy(r => (int)Enum.Parse<DayOfWeek>(r.Dia))
                .ToList();

            return resultado;
        }

        public async Task<decimal> ObtenerCrecimientoAnualAsync()
        {
            var facturas = await _controlador.ObtenerTodasFacturasAsync();
            int anioActual = DateTime.Now.Year;

            decimal totalActual = facturas
                .Where(f => f.FechaFactura.Year == anioActual)
                .Sum(f => f.Total);

            decimal totalAnterior = facturas
                .Where(f => f.FechaFactura.Year == anioActual - 1)
                .Sum(f => f.Total);

            if (totalAnterior == 0)
            {
                return totalActual > 0 ? 1.00m : 0.00m; 
            }
            return (totalActual - totalAnterior) / totalAnterior;
        }
    }
}