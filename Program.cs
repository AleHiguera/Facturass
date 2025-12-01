using blazor.Components;
using blazor.Components.Data;
using blazor.Components.Servicios;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

// --- USINGS AGREGADOS PARA RESOLVER EL ERROR 'WebApplication' ---
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Definición de la ruta de la base de datos
String ruta = "mibase_facturas.db";

// Configuración de la cadena de conexión
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "ConnectionStrings:DefaultConnection", $"Data Source={ruta}" }
});

// Registro del ServicioFacturas (acceso a DB)
builder.Services.AddTransient<ServicioFacturas>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    // Usamos el operador ??= para asegurar que no sea nulo.
    string connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    return new ServicioFacturas(connectionString);
});

// Registro de los servicios de lógica
builder.Services.AddTransient<ServicioControlador>();
builder.Services.AddTransient<ServicioDashboard>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/404");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using var conexion = new SqliteConnection($"Data Source={ruta}");
conexion.Open();
var comando = conexion.CreateCommand();

comando.CommandText = """
    CREATE TABLE IF NOT EXISTS Facturas (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FechaFactura TEXT NOT NULL,
        NombreCliente TEXT NOT NULL,
        Total REAL NOT NULL DEFAULT 0.00
    );
    """;
comando.ExecuteNonQuery();

try
{
    comando.CommandText = "ALTER TABLE Facturas ADD COLUMN Archivada INTEGER NOT NULL DEFAULT 0;";
    comando.ExecuteNonQuery();
}
catch (SqliteException ex) when (ex.Message.Contains("duplicate column name"))
{
}
catch (Exception ex)
{
    Console.WriteLine($"Error al intentar modificar la tabla Facturas: {ex.Message}");
}


comando.CommandText = """
    CREATE TABLE IF NOT EXISTS ArticulosFactura (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FacturaId INTEGER NOT NULL,
        Descripcion TEXT NOT NULL,
        Cantidad INTEGER NOT NULL, 
        Precio REAL NOT NULL,
        FOREIGN KEY(FacturaId) REFERENCES Facturas(Id) ON DELETE CASCADE
    );
    """;
comando.ExecuteNonQuery();

comando.CommandText = """
    CREATE TABLE IF NOT EXISTS configuracion (
        clave TEXT PRIMARY KEY,
        valor TEXT
    );
    """;
comando.ExecuteNonQuery();

comando.CommandText = """
    INSERT OR IGNORE INTO configuracion (clave, valor) 
    VALUES ('FiltroNombreCliente', '');
    """;
comando.ExecuteNonQuery();

conexion.Close();


app.Run();