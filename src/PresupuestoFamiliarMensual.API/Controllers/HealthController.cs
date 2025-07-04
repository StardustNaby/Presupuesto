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
} 