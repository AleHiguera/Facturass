using blazor.Components.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
namespace blazor.Components.Servicios
{
    public class ServicioControlador
    {
        private readonly ServicioFacturas _servicioFacturas;

        private const string CLAVE_FILTRO_CLIENTE = "FiltroNombreCliente";

        public string FiltroNombreCliente { get; set; } = string.Empty;

        private List<Factura> _facturasEnMemoria = new List<Factura>();

        public ServicioControlador(ServicioFacturas servicioFacturas)
        {
            _servicioFacturas = servicioFacturas;
        }
        public async Task CargarFiltroAsync()
        {
            FiltroNombreCliente = await _servicioFacturas.ObtenerConfiguracionAsync(CLAVE_FILTRO_CLIENTE);
        }

        public async Task CargarFacturasAsync()
        {
            _facturasEnMemoria = (await _servicioFacturas.ObtenerTodasAsync()).ToList();
        }

        public IEnumerable<Factura> ObtenerFacturasFiltradas()
        {
            IEnumerable<Factura> resultado = _facturasEnMemoria;

            if (!string.IsNullOrWhiteSpace(FiltroNombreCliente))
            {
                resultado = resultado.Where(f =>
                    f.NombreCliente.Contains(FiltroNombreCliente, StringComparison.OrdinalIgnoreCase));
            }

            Task.Run(async () =>
            {
                await _servicioFacturas.GuardarConfiguracionAsync(CLAVE_FILTRO_CLIENTE, FiltroNombreCliente);
            }).FireAndForget();

            return resultado.OrderByDescending(f => f.FechaFactura);
        }

        public async Task GuardarNuevaFacturaAsync(Factura nuevaFactura)
        {
            await _servicioFacturas.AgregarFacturaAsync(nuevaFactura);
        }

        public async Task<Factura?> ObtenerFacturaPorIdAsync(int id)
        {
            var facturaEnMemoria = _facturasEnMemoria.FirstOrDefault(f => f.Id == id);
            if (facturaEnMemoria != null)
            {
                return facturaEnMemoria;
            }

            var facturaDesdeDB = await _servicioFacturas.ObtenerPorIdAsync(id);

            if (facturaDesdeDB != null)
            {
                _facturasEnMemoria.Add(facturaDesdeDB);
            }

            return facturaDesdeDB;
        }
    }
    public static class TaskExtension
    {
        public static void FireAndForget(this Task task)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en tarea FireAndForget: {ex.Message}");
                }
            });
        }
    }
}