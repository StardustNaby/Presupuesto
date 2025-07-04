using Microsoft.AspNetCore.Mvc;

namespace PresupuestoFamiliarMensual.API.Controllers;

/// <summary>
/// Controlador de health ultra simple para Railway
/// </summary>
[ApiController]
[Route("api/health")]
public class SimpleHealthController : ControllerBase
{
    /// <summary>
    /// Health check ultra simple
    /// </summary>
    /// <returns>OK</returns>
    [HttpGet("simple")]
    public ActionResult Get()
    {
        return Ok("OK");
    }

    /// <summary>
    /// Health check básico
    /// </summary>
    /// <returns>OK</returns>
    [HttpGet("basic")]
    public ActionResult GetBasic()
    {
        return Ok("OK");
    }

    /// <summary>
    /// Health check para Railway
    /// </summary>
    /// <returns>OK</returns>
    [HttpGet("railway")]
    public ActionResult GetRailway()
    {
        return Ok("OK");
    }
} 