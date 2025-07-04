using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Application.Mapping;
using PresupuestoFamiliarMensual.Application.Services;
using PresupuestoFamiliarMensual.Core.Interfaces;
using PresupuestoFamiliarMensual.Infrastructure.Data;
using PresupuestoFamiliarMensual.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración específica para Railway
var port = Environment.GetEnvironmentVariable("PORT");
Console.WriteLine($"🚀 Puerto detectado: {port ?? "null"}");

// Configurar el puerto para Railway
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int portNumber))
{
    Console.WriteLine($"✅ Usando puerto: {portNumber}");
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}
else
{
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

builder.Services.AddEndpointsApiExplorer();

// Swagger siempre habilitado (desarrollo y producción)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Presupuesto Familiar Mensual API", Version = "v1" });
});

// Database - Configuración condicional para Railway
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    // Si la cadena es formato URL (postgres:// o postgresql://), la convertimos a formato largo
    if (dbUrl.StartsWith("postgres://") || dbUrl.StartsWith("postgresql://"))
    {
        var url = dbUrl.Replace("postgresql://", "postgres://");
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var builderStr = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        connectionString = builderStr;
        Console.WriteLine($"🔗 DATABASE_URL detectado en formato URL, convertido a formato largo para Npgsql");
    }
    else
    {
        connectionString = dbUrl;
        Console.WriteLine($"🔗 Usando cadena de conexión de DATABASE_URL");
    }
}
else
{
    Console.WriteLine($"🔗 Usando cadena de conexión de appsettings.json");
}
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine($"Cadena de conexión final: {connectionString}");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, 
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 2,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorCodesToAdd: null)));
}
else
{
    Console.WriteLine("⚠️ Sin base de datos - modo de prueba");
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

// Configuración de JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Registrar servicios de autenticación y JWT
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configurar autenticación JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
}
else
{
    Console.WriteLine("⚠️ JwtSettings no encontrado en configuración, autenticación JWT deshabilitada");
}

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

// Ejecutar migraciones automáticamente al iniciar
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        Console.WriteLine("🔄 Ejecutando migraciones automáticamente...");
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Migraciones ejecutadas correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al ejecutar migraciones: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Presupuesto Familiar Mensual API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Middleware de manejo de excepciones global simplificado
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Error interno del servidor");
    }
});

Console.WriteLine("✅ Aplicación iniciada correctamente");
app.Run(); 