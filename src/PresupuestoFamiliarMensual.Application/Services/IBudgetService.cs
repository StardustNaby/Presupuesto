using PresupuestoFamiliarMensual.Application.DTOs;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Interfaz para el servicio de presupuestos
/// </summary>
public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllAsync();
    Task<PaginatedResponse<BudgetDto>> GetPaginatedAsync(PaginationParameters parameters);
    Task<BudgetDto?> GetByIdAsync(int id);
    Task<IEnumerable<BudgetDto>> GetByFamilyMemberAsync(int familyMemberId);
    Task<PaginatedResponse<BudgetDto>> GetByFamilyMemberPaginatedAsync(int familyMemberId, PaginationParameters parameters);
    Task<BudgetDto> CreateAsync(CreateBudgetDto createBudgetDto);
    Task<BudgetDto> UpdateAsync(int id, CreateBudgetDto updateBudgetDto);
    Task DeleteAsync(int id);
} 