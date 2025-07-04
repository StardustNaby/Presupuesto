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
using Microsoft.OpenApi.Models;
using System.Reflection;
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

// Configuración adicional para Railway
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(portNumber > 0 ? portNumber : 8080);
});

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
    c.SwaggerDoc("v1", new()
    {
        Title = "Presupuesto Familiar Mensual API",
        Version = "v1.0.0",
        Description = "API completa para gestión de presupuestos familiares mensuales. Incluye autenticación JWT, gestión de presupuestos, categorías y gastos con validaciones de negocio.",
        Contact = new()
        {
            Name = "Equipo de Desarrollo",
            Email = "desarrollo@presupuestofamiliar.com"
        },
        License = new()
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML si existen
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
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

// Health Checks básicos
builder.Services.AddHealthChecks();

var app = builder.Build();

// Las migraciones se ejecutarán manualmente cuando sea necesario
Console.WriteLine("🚀 Aplicación iniciada - migraciones disponibles para ejecución manual");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Presupuesto Familiar Mensual API v1.0.0");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Presupuesto Familiar Mensual - Documentación API";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseCors("AllowAll");

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint simple
app.MapHealthChecks("/health");

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