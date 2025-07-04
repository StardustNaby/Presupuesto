using AutoMapper;
using PresupuestoFamiliarMensual.Application.DTOs;
using PresupuestoFamiliarMensual.Core.Entities;
using PresupuestoFamiliarMensual.Core.Interfaces;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Implementación del servicio de presupuestos
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BudgetService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BudgetDto>> GetAllAsync()
    {
        var budgets = await _unitOfWork.Budgets.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
    }

    public async Task<PaginatedResponse<BudgetDto>> GetPaginatedAsync(PaginationParameters parameters)
    {
        var query = await _unitOfWork.Budgets.GetAllWithDetailsAsync();
        var budgets = query.ToList();

        // Aplicar búsqueda si se especifica
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            budgets = budgets.Where(b => 
                b.FamilyMember?.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                b.Month?.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            budgets = parameters.SortBy.ToLower() switch
            {
                "totalamount" => parameters.SortDirection?.ToLower() == "desc" 
                    ? budgets.OrderByDescending(b => b.TotalAmount).ToList()
                    : budgets.OrderBy(b => b.TotalAmount).ToList(),
                "createdat" => parameters.SortDirection?.ToLower() == "desc"
                    ? budgets.OrderByDescending(b => b.CreatedAt).ToList()
                    : budgets.OrderBy(b => b.CreatedAt).ToList(),
                "familymember" => parameters.SortDirection?.ToLower() == "desc"
                    ? budgets.OrderByDescending(b => b.FamilyMember?.Name).ToList()
                    : budgets.OrderBy(b => b.FamilyMember?.Name).ToList(),
                "month" => parameters.SortDirection?.ToLower() == "desc"
                    ? budgets.OrderByDescending(b => b.Month?.Name).ToList()
                    : budgets.OrderBy(b => b.Month?.Name).ToList(),
                _ => budgets.OrderBy(b => b.CreatedAt).ToList()
            };
        }
        else
        {
            budgets = budgets.OrderBy(b => b.CreatedAt).ToList();
        }

        var totalCount = budgets.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var currentPage = parameters.PageNumber;
        var pageSize = parameters.PageSize;

        var pagedData = budgets
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var budgetDtos = _mapper.Map<IEnumerable<BudgetDto>>(pagedData);

        return new PaginatedResponse<BudgetDto>
        {
            Data = budgetDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Pagination = new PaginationInfo
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                HasPreviousPage = currentPage > 1,
                HasNextPage = currentPage < totalPages
            }
        };
    }

    public async Task<BudgetDto?> GetByIdAsync(int id)
    {
        var budget = await _unitOfWork.Budgets.GetByIdWithDetailsAsync(id);
        return _mapper.Map<BudgetDto>(budget);
    }

    public async Task<IEnumerable<BudgetDto>> GetByFamilyMemberAsync(int familyMemberId)
    {
        var budgets = await _unitOfWork.Budgets.GetByFamilyMemberAsync(familyMemberId);
        return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
    }

    public async Task<PaginatedResponse<BudgetDto>> GetByFamilyMemberPaginatedAsync(int familyMemberId, PaginationParameters parameters)
    {
        var query = await _unitOfWork.Budgets.GetByFamilyMemberAsync(familyMemberId);
        var budgets = query.ToList();

        // Aplicar búsqueda si se especifica
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            budgets = budgets.Where(b => 
                b.Month?.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            budgets = parameters.SortBy.ToLower() switch
            {
                "totalamount" => parameters.SortDirection?.ToLower() == "desc" 
                    ? budgets.OrderByDescending(b => b.TotalAmount).ToList()
                    : budgets.OrderBy(b => b.TotalAmount).ToList(),
                "createdat" => parameters.SortDirection?.ToLower() == "desc"
                    ? budgets.OrderByDescending(b => b.CreatedAt).ToList()
                    : budgets.OrderBy(b => b.CreatedAt).ToList(),
                "month" => parameters.SortDirection?.ToLower() == "desc"
                    ? budgets.OrderByDescending(b => b.Month?.Name).ToList()
                    : budgets.OrderBy(b => b.Month?.Name).ToList(),
                _ => budgets.OrderBy(b => b.CreatedAt).ToList()
            };
        }
        else
        {
            budgets = budgets.OrderBy(b => b.CreatedAt).ToList();
        }

        var totalCount = budgets.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var currentPage = parameters.PageNumber;
        var pageSize = parameters.PageSize;

        var pagedData = budgets
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var budgetDtos = _mapper.Map<IEnumerable<BudgetDto>>(pagedData);

        return new PaginatedResponse<BudgetDto>
        {
            Data = budgetDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Pagination = new PaginationInfo
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                HasPreviousPage = currentPage > 1,
                HasNextPage = currentPage < totalPages
            }
        };
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetDto createBudgetDto)
    {
        // Verificar que el miembro de la familia existe
        var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(createBudgetDto.FamilyMemberId);
        if (familyMember == null)
            throw new ArgumentException($"No se encontró el miembro de la familia con ID {createBudgetDto.FamilyMemberId}");

        // Verificar que el mes existe
        var month = await _unitOfWork.Months.GetByIdAsync(createBudgetDto.MonthId);
        if (month == null)
            throw new ArgumentException($"No se encontró el mes con ID {createBudgetDto.MonthId}");

        // Verificar que no existe ya un presupuesto para este miembro y mes
        var existingBudget = await _unitOfWork.Budgets.GetByFamilyMemberAndMonthAsync(
            createBudgetDto.FamilyMemberId, createBudgetDto.MonthId);
        
        if (existingBudget != null)
            throw new InvalidOperationException($"Ya existe un presupuesto para este miembro de la familia en el mes seleccionado");

        var budget = new Budget
        {
            TotalAmount = createBudgetDto.TotalAmount,
            FamilyMemberId = createBudgetDto.FamilyMemberId,
            MonthId = createBudgetDto.MonthId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Budgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        var createdBudget = await _unitOfWork.Budgets.GetByIdWithDetailsAsync(budget.Id);
        return _mapper.Map<BudgetDto>(createdBudget);
    }

    public async Task<BudgetDto> UpdateAsync(int id, CreateBudgetDto updateBudgetDto)
    {
        var budget = await _unitOfWork.Budgets.GetByIdAsync(id);
        if (budget == null)
            throw new ArgumentException($"No se encontró el presupuesto con ID {id}");

        // Verificar que el miembro de la familia existe
        var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(updateBudgetDto.FamilyMemberId);
        if (familyMember == null)
            throw new ArgumentException($"No se encontró el miembro de la familia con ID {updateBudgetDto.FamilyMemberId}");

        // Verificar que el mes existe
        var month = await _unitOfWork.Months.GetByIdAsync(updateBudgetDto.MonthId);
        if (month == null)
            throw new ArgumentException($"No se encontró el mes con ID {updateBudgetDto.MonthId}");

        // Verificar que no existe ya un presupuesto para este miembro y mes (excluyendo el actual)
        var existingBudget = await _unitOfWork.Budgets.GetByFamilyMemberAndMonthAsync(
            updateBudgetDto.FamilyMemberId, updateBudgetDto.MonthId);
        
        if (existingBudget != null && existingBudget.Id != id)
            throw new InvalidOperationException($"Ya existe un presupuesto para este miembro de la familia en el mes seleccionado");

        budget.TotalAmount = updateBudgetDto.TotalAmount;
        budget.FamilyMemberId = updateBudgetDto.FamilyMemberId;
        budget.MonthId = updateBudgetDto.MonthId;
        budget.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Budgets.UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        var updatedBudget = await _unitOfWork.Budgets.GetByIdWithDetailsAsync(budget.Id);
        return _mapper.Map<BudgetDto>(updatedBudget);
    }

    public async Task DeleteAsync(int id)
    {
        var budget = await _unitOfWork.Budgets.GetByIdAsync(id);
        if (budget == null)
            throw new ArgumentException($"No se encontró el presupuesto con ID {id}");

        await _unitOfWork.Budgets.DeleteAsync(budget);
        await _unitOfWork.SaveChangesAsync();
    }
} 