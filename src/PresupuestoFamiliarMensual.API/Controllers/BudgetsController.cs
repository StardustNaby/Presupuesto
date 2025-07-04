using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PresupuestoFamiliarMensual.Application.DTOs;
using PresupuestoFamiliarMensual.Application.Services;
using PresupuestoFamiliarMensual.Core.Exceptions;

namespace PresupuestoFamiliarMensual.API.Controllers;

/// <summary>
/// Controlador para gestionar presupuestos mensuales
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    /// <summary>
    /// Obtiene todos los presupuestos del usuario autenticado
    /// </summary>
    /// <returns>Lista completa de presupuestos</returns>
    /// <response code="200">Lista de presupuestos obtenida exitosamente</response>
    /// <response code="401">No autorizado - Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAll()
    {
        try
        {
            var budgets = await _budgetService.GetAllAsync();
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene presupuestos con paginación, ordenamiento y búsqueda
    /// </summary>
    /// <param name="pageNumber">Número de página (comienza en 1)</param>
    /// <param name="pageSize">Tamaño de la página (máximo 50)</param>
    /// <param name="sortBy">Campo por el cual ordenar (totalamount, createdat, familymember, month)</param>
    /// <param name="sortDirection">Dirección del ordenamiento (asc/desc)</param>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <returns>Presupuestos paginados con metadatos</returns>
    /// <response code="200">Presupuestos paginados obtenidos exitosamente</response>
    /// <response code="401">No autorizado - Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("paginated")]
    [ProducesResponseType(typeof(PaginatedResponse<BudgetDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<PaginatedResponse<BudgetDto>>> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection,
                SearchTerm = searchTerm
            };

            var result = await _budgetService.GetPaginatedAsync(parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un presupuesto por su ID
    /// </summary>
    /// <param name="id">ID del presupuesto</param>
    /// <returns>Presupuesto encontrado</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetDto>> GetById(int id)
    {
        try
        {
            var budget = await _budgetService.GetByIdAsync(id);
            if (budget == null)
                return NotFound(new { message = $"No se encontró el presupuesto con ID {id}" });

            return Ok(budget);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene presupuestos por miembro de la familia
    /// </summary>
    /// <param name="familyMemberId">ID del miembro de la familia</param>
    /// <returns>Lista de presupuestos del miembro</returns>
    [HttpGet("family-member/{familyMemberId}")]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetByFamilyMember(int familyMemberId)
    {
        try
        {
            var budgets = await _budgetService.GetByFamilyMemberAsync(familyMemberId);
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene presupuestos por miembro de la familia con paginación
    /// </summary>
    /// <param name="familyMemberId">ID del miembro de la familia</param>
    /// <param name="pageNumber">Número de página (comienza en 1)</param>
    /// <param name="pageSize">Tamaño de la página (máximo 50)</param>
    /// <param name="sortBy">Campo por el cual ordenar (totalamount, createdat, month)</param>
    /// <param name="sortDirection">Dirección del ordenamiento (asc/desc)</param>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <returns>Presupuestos del miembro paginados</returns>
    [HttpGet("family-member/{familyMemberId}/paginated")]
    public async Task<ActionResult<PaginatedResponse<BudgetDto>>> GetByFamilyMemberPaginated(
        int familyMemberId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection,
                SearchTerm = searchTerm
            };

            var result = await _budgetService.GetByFamilyMemberPaginatedAsync(familyMemberId, parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo presupuesto
    /// </summary>
    /// <param name="createBudgetDto">Datos del presupuesto a crear</param>
    /// <returns>Presupuesto creado</returns>
    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(CreateBudgetDto createBudgetDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var budget = await _budgetService.CreateAsync(createBudgetDto);
            return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un presupuesto existente
    /// </summary>
    /// <param name="id">ID del presupuesto</param>
    /// <param name="updateBudgetDto">Datos actualizados del presupuesto</param>
    /// <returns>Presupuesto actualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<BudgetDto>> Update(int id, CreateBudgetDto updateBudgetDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var budget = await _budgetService.UpdateAsync(id, updateBudgetDto);
            return Ok(budget);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un presupuesto
    /// </summary>
    /// <param name="id">ID del presupuesto</param>
    /// <returns>Sin contenido</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _budgetService.DeleteAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
} 