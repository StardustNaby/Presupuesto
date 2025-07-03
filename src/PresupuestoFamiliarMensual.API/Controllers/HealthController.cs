using Microsoft.AspNetCore.Mvc;

namespace PresupuestoFamiliarMensual.API.Controllers;

/// <summary>
/// Controlador para health checks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
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
            service = "Presupuesto Familiar Mensual API",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }

    /// <summary>
    /// Health check simple para Railway
    /// </summary>
    /// <returns>Estado de la aplicación</returns>
    [HttpGet("simple")]
    public ActionResult GetSimple()
    {
        return Ok("OK");
    }
} 