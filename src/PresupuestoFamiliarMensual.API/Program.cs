using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuración específica para Railway
var port = Environment.GetEnvironmentVariable("PORT");
Console.WriteLine($"🚀 Puerto detectado: {port ?? "null"}");

// Configurar el puerto para Railway
int portNumber = 8080; // Puerto por defecto
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int parsedPort))
{
    portNumber = parsedPort;
    Console.WriteLine($"✅ Usando puerto: {portNumber}");
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}
else
{
    Console.WriteLine($"⚠️ Usando puerto por defecto: {portNumber}");
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}

// Configuración adicional para Railway
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(portNumber);
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger básico
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Presupuesto Familiar Mensual API", Version = "v1" });
});

// Health Checks básicos
builder.Services.AddHealthChecks();

var app = builder.Build();

Console.WriteLine("🚀 Aplicación iniciada - versión minimalista");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Presupuesto Familiar Mensual API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

// Health check endpoint simple
app.MapHealthChecks("/health");

Console.WriteLine("✅ Aplicación iniciada correctamente");
app.Run(); 