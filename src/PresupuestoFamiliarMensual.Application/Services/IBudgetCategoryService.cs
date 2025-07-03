using PresupuestoFamiliarMensual.Application.DTOs;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Interfaz para el servicio de categorías de presupuesto
/// </summary>
public interface IBudgetCategoryService
{
    Task<IEnumerable<BudgetCategoryDto>> GetByBudgetIdAsync(int budgetId);
    Task<BudgetCategoryDto?> GetByIdAsync(int id);
    Task<BudgetCategoryDto> CreateAsync(int budgetId, CreateBudgetCategoryDto createCategoryDto);
    Task<BudgetCategoryDto> UpdateAsync(int id, UpdateBudgetCategoryDto updateCategoryDto);
    Task DeleteAsync(int id);
} 