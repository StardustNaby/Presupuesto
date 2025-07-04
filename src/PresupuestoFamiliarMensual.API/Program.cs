using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuración básica para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Puerto: {port}");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configuración de base de datos
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    Console.WriteLine("🔗 Configurando base de datos...");
    
    // Convertir formato URL a formato de conexión
    if (dbUrl.StartsWith("postgres://") || dbUrl.StartsWith("postgresql://"))
    {
        var url = dbUrl.Replace("postgresql://", "postgres://");
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        Console.WriteLine("✅ Base de datos PostgreSQL configurada");
    }
    else
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dbUrl));
        Console.WriteLine("✅ Base de datos configurada con cadena directa");
    }
}
else
{
    Console.WriteLine("⚠️ DATABASE_URL no encontrado - modo sin base de datos");
}

// Servicios mínimos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

Console.WriteLine("🚀 Aplicación iniciada");

// Ejecutar migraciones si hay base de datos
if (!string.IsNullOrEmpty(dbUrl))
{
    try
    {
        Console.WriteLine("🔄 Ejecutando migraciones...");
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Migraciones ejecutadas correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error en migraciones: {ex.Message}");
    }
}

// Pipeline mínimo
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("✅ Aplicación lista");
app.Run(); 