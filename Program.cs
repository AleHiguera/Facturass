using blazor.Components;
using blazor.Components.Data;
using blazor.Components.Servicios;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

String ruta = "mibase_facturas.db";

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    { "ConnectionStrings:DefaultConnection", $"Data Source={ruta}" }
});

builder.Services.AddTransient<ServicioFacturas>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    return new ServicioFacturas(connectionString);
});

builder.Services.AddTransient<ServicioControlador>();


var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
        NombreCliente TEXT NOT NULL
    );
    """;
comando.ExecuteNonQuery();

comando.CommandText = """
    CREATE TABLE IF NOT EXISTS ArticulosFactura (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FacturaId INTEGER NOT NULL,
        Descripcion TEXT NOT NULL,
        Cantidad INTEGER NOT NULL, 
        Precio REAL NOT NULL,
        FOREIGN KEY(FacturaId) REFERENCES Facturas(Id)
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