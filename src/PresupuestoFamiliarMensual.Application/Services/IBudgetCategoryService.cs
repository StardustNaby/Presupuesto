using PresupuestoFamiliarMensual.Application.DTOs;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Interfaz para el servicio de categorías de presupuesto
/// </summary>
public interface IBudgetCategoryService
{
    Task<IEnumerable<BudgetCategoryDto>> GetAllAsync();
    Task<PaginatedResponse<BudgetCategoryDto>> GetPaginatedAsync(PaginationParameters parameters);
    Task<IEnumerable<BudgetCategoryDto>> GetByBudgetIdAsync(int budgetId);
    Task<PaginatedResponse<BudgetCategoryDto>> GetByBudgetIdPaginatedAsync(int budgetId, PaginationParameters parameters);
    Task<BudgetCategoryDto?> GetByIdAsync(int id);
    Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryDto createCategoryDto);
    Task<BudgetCategoryDto> UpdateAsync(int id, UpdateBudgetCategoryDto updateCategoryDto);
    Task DeleteAsync(int id);
} 