using Microsoft.AspNetCore.Mvc;
using PresupuestoFamiliarMensual.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PresupuestoFamiliarMensual.API.Controllers;

/// <summary>
/// Controlador para health checks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Health check básico para Railway
    /// </summary>
    /// <returns>Estado de la aplicación</returns>
    [HttpGet]
    public ActionResult Get()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "Presupuesto Familiar Mensual API"
        });
    }

    /// <summary>
    /// Health check simple para Railway - optimizado para velocidad
    /// </summary>
    /// <returns>Estado de la aplicación</returns>
    [HttpGet("simple")]
    public ActionResult GetSimple()
    {
        return Ok("OK");
    }

    /// <summary>
    /// Health check más simple posible
    /// </summary>
    /// <returns>Estado de la aplicación</returns>
    [HttpGet("basic")]
    public ActionResult GetBasic()
    {
        return Ok("OK");
    }

    /// <summary>
    /// Health check ultra simple para Railway
    /// </summary>
    /// <returns>Estado de la aplicación</returns>
    [HttpGet("railway")]
    public ActionResult GetRailway()
    {
        return Ok("OK");
    }

    [HttpGet("ping")]
    public ActionResult Ping()
    {
        return Ok("pong");
    }

    /// <summary>
    /// Endpoint de diagnóstico para verificar el estado de la aplicación
    /// </summary>
    /// <returns>Información de diagnóstico</returns>
    [HttpGet("diagnostic")]
    public ActionResult GetDiagnostic()
    {
        var diagnostic = new
        {
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Configured" : "Not configured",
            port = Environment.GetEnvironmentVariable("PORT") ?? "8080",
            machineName = Environment.MachineName,
            osVersion = Environment.OSVersion.ToString(),
            dotnetVersion = Environment.Version.ToString()
        };

        return Ok(diagnostic);
    }

    /// <summary>
    /// Endpoint de diagnóstico detallado de la base de datos
    /// </summary>
    /// <returns>Información detallada de la base de datos</returns>
    [HttpGet("database")]
    public async Task<ActionResult> GetDatabaseDiagnostic()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            
            var diagnostic = new
            {
                timestamp = DateTime.UtcNow,
                canConnect = canConnect,
                pendingMigrations = pendingMigrations.ToArray(),
                appliedMigrations = appliedMigrations.ToArray(),
                pendingMigrationsCount = pendingMigrations.Count(),
                appliedMigrationsCount = appliedMigrations.Count(),
                connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Configured" : "Not configured"
            };

            return Ok(diagnostic);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                timestamp = DateTime.UtcNow,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
} 