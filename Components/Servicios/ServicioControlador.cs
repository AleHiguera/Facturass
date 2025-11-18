using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blazor.Components.Data;
using static blazor.Components.Data.Factura;

namespace blazor.Components.Servicios
{
    public enum CriterioOrdenacion
    {
        FechaDescendente,
        IdDescendente,
        NombreClienteAscendente
    }

    public class ServicioControlador
    {
        private readonly ServicioFacturas _servicioFacturas;

        private const string CLAVE_FILTRO_CLIENTE = "FiltroNombreCliente";
        private const string CLAVE_ORDENACION = "CriterioOrdenacion";

        public string FiltroNombreCliente { get; set; } = string.Empty;

        public CriterioOrdenacion OrdenacionSeleccionada { get; set; } = CriterioOrdenacion.FechaDescendente;

        private List<Factura> _facturasEnMemoria = new List<Factura>();

        public ServicioControlador(ServicioFacturas servicioFacturas)
        {
            _servicioFacturas = servicioFacturas;
        }

        public async Task CargarFiltroAsync()
        {
            FiltroNombreCliente = await _servicioFacturas.ObtenerConfiguracionAsync(CLAVE_FILTRO_CLIENTE);

            var ordenacionGuardada = await _servicioFacturas.ObtenerConfiguracionAsync(CLAVE_ORDENACION);
            if (Enum.TryParse(ordenacionGuardada, out CriterioOrdenacion orden))
            {
                OrdenacionSeleccionada = orden;
            }
        }

        public async Task CargarFacturasAsync()
        {
            // Este método siempre va a la DB y reemplaza la caché
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

            resultado = OrdenacionSeleccionada switch
            {
                CriterioOrdenacion.IdDescendente => resultado.OrderByDescending(f => f.Id),
                CriterioOrdenacion.NombreClienteAscendente => resultado.OrderBy(f => f.NombreCliente),
                CriterioOrdenacion.FechaDescendente or _ => resultado.OrderByDescending(f => f.FechaFactura),
            };

            Task.Run(async () =>
            {
                await _servicioFacturas.GuardarConfiguracionAsync(CLAVE_FILTRO_CLIENTE, FiltroNombreCliente);
                await _servicioFacturas.GuardarConfiguracionAsync(CLAVE_ORDENACION, OrdenacionSeleccionada.ToString());
            }).FireAndForget();

            return resultado;
        }

        public async Task GuardarNuevaFacturaAsync(Factura nuevaFactura)
        {
            await _servicioFacturas.AgregarFacturaAsync(nuevaFactura);
        }

        public async Task ActualizarFacturaAsync(Factura facturaEditada)
        {
            await _servicioFacturas.ActualizarFacturaAsync(facturaEditada);

            var index = _facturasEnMemoria.FindIndex(f => f.Id == facturaEditada.Id);
            if (index != -1)
            {
                _facturasEnMemoria[index] = facturaEditada;
            }
        }

        public async Task EliminarFacturaAsync(int facturaId)
        {
            await _servicioFacturas.EliminarFacturaAsync(facturaId);
            var facturaAEliminar = _facturasEnMemoria.FirstOrDefault(f => f.Id == facturaId);
            if (facturaAEliminar != null)
            {
                _facturasEnMemoria.Remove(facturaAEliminar);
            }
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

        public async Task<IEnumerable<Factura>> ObtenerTodasFacturasAsync()
        {
            if (!_facturasEnMemoria.Any())
            {
                await CargarFacturasAsync();
            }
            return _facturasEnMemoria;
        }
        public async Task<IEnumerable<ReporteMensual>> ObtenerReporteAnualAsync(int anio)
        {
            await CargarFacturasAsync();

            return await _servicioFacturas.ObtenerReporteAnualAsync(anio);
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