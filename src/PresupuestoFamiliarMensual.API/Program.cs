using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Application.Mapping;
using PresupuestoFamiliarMensual.Application.Services;
using PresupuestoFamiliarMensual.Core.Interfaces;
using PresupuestoFamiliarMensual.Infrastructure.Data;
using PresupuestoFamiliarMensual.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuración específica para Railway
var port = Environment.GetEnvironmentVariable("PORT");
Console.WriteLine($"🚀 Puerto detectado: {port ?? "null"}");

if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int portNumber))
{
    Console.WriteLine($"✅ Usando puerto: {portNumber}");
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}
else
{
    // Puerto por defecto si no se puede parsear
    Console.WriteLine($"⚠️ Usando puerto por defecto: 8080");
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

// Configurar variables de entorno para Railway
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Presupuesto Familiar Mensual API", Version = "v1" });
});

// Database - Configuración condicional para Railway
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, 
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null)));
}
else
{
    // Modo de prueba sin base de datos
    Console.WriteLine("⚠️ No se encontró cadena de conexión. Ejecutando en modo de prueba.");
}

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Repositories
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBudgetCategoryRepository, BudgetCategoryRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Habilitar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Presupuesto Familiar Mensual API v1");
        c.RoutePrefix = "swagger";
    });
}

// CORS debe ir antes que HTTPS redirection
app.UseCors("AllowAll");

// Solo usar HTTPS redirection en producción
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Middleware de manejo de excepciones global
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error no manejado: {ex.Message}");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            message = "Error interno del servidor",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        }));
    }
});

// Seed data solo en desarrollo y si la base de datos está disponible
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedDataAsync(context);
        }
        catch (Exception ex)
        {
            // Log el error pero no fallar la aplicación
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }
}

Console.WriteLine("✅ Aplicación iniciada correctamente");
app.Run();

// Método para sembrar datos iniciales
static async Task SeedDataAsync(ApplicationDbContext context)
{
    if (!context.Months.Any())
    {
        var months = new[]
        {
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 1, Year = 2024, Name = "Enero" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 2, Year = 2024, Name = "Febrero" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 3, Year = 2024, Name = "Marzo" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 4, Year = 2024, Name = "Abril" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 5, Year = 2024, Name = "Mayo" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 6, Year = 2024, Name = "Junio" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 7, Year = 2024, Name = "Julio" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 8, Year = 2024, Name = "Agosto" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 9, Year = 2024, Name = "Septiembre" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 10, Year = 2024, Name = "Octubre" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 11, Year = 2024, Name = "Noviembre" },
            new PresupuestoFamiliarMensual.Core.Entities.Month { MonthNumber = 12, Year = 2024, Name = "Diciembre" }
        };
        
        context.Months.AddRange(months);
        await context.SaveChangesAsync();
    }

    if (!context.FamilyMembers.Any())
    {
        var familyMembers = new[]
        {
            new PresupuestoFamiliarMensual.Core.Entities.FamilyMember { Name = "Juan Pérez", Email = "juan@familia.com" },
            new PresupuestoFamiliarMensual.Core.Entities.FamilyMember { Name = "María Pérez", Email = "maria@familia.com" },
            new PresupuestoFamiliarMensual.Core.Entities.FamilyMember { Name = "Carlos Pérez", Email = "carlos@familia.com" }
        };
        
        context.FamilyMembers.AddRange(familyMembers);
        await context.SaveChangesAsync();
    }
} 