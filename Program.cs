using blazor.Components;
using blazor.Components.Data;
using blazor.Components.Servicios;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration; // Asegúrate de que este using esté presente

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Define la ruta y añade la cadena de conexión a la Configuración (Estilo del ejemplo)
String ruta = "mibase_facturas.db";

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    // Usamos Data Source=ruta para SQLite
    { "ConnectionStrings:DefaultConnection", $"Data Source={ruta}" }
});

// 2. Registrar Servicios con AddTransient (Estilo del ejemplo)
// Usamos una función de fábrica para que el servicio pueda obtener la cadena de conexión
// de la configuración cada vez que se crea (Transient).
builder.Services.AddTransient<ServicioFacturas>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    return new ServicioFacturas(connectionString);
});

// ServicioControlador (asumiendo que se sigue el patrón Transient)
builder.Services.AddTransient<ServicioControlador>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery(); // Añadido Antiforgery como en el ejemplo

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 3. Inicialización de la Base de Datos (Creation of Tables)
try
{
    // Obtenemos la cadena de conexión desde la configuración
    string connectionString = app.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string is missing for DB initialization.");

    using var conexion = new SqliteConnection(connectionString);
    conexion.Open();

    var comando = conexion.CreateCommand();

    // Tabla Facturas
    comando.CommandText = """
        CREATE TABLE IF NOT EXISTS Facturas (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FechaFactura TEXT NOT NULL,
            NombreCliente TEXT NOT NULL
        );
        """;
    comando.ExecuteNonQuery();

    // Tabla ArticulosFactura
    comando.CommandText = """
        CREATE TABLE IF NOT EXISTS ArticulosFactura (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FacturaId INTEGER NOT NULL,
            Descripcion TEXT NOT NULL,
            Precio REAL NOT NULL,
            FOREIGN KEY(FacturaId) REFERENCES Facturas(Id)
        );
        """;
    comando.ExecuteNonQuery();
}
catch (Exception ex)
{
    // Es buena práctica usar el logger, pero se mantiene Console.WriteLine del original
    Console.WriteLine("ERROR BASE DE DATOS: " + ex.Message);
}

app.Run();