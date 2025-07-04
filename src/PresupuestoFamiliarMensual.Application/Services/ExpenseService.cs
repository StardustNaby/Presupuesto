using AutoMapper;
using PresupuestoFamiliarMensual.Application.DTOs;
using PresupuestoFamiliarMensual.Core.Entities;
using PresupuestoFamiliarMensual.Core.Exceptions;
using PresupuestoFamiliarMensual.Core.Interfaces;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Implementación del servicio de gastos
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ExpenseDto>> GetByBudgetIdAsync(int budgetId)
    {
        var expenses = await _unitOfWork.Expenses.GetByBudgetIdAsync(budgetId);
        return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
    }

    public async Task<PaginatedResponse<ExpenseDto>> GetByBudgetIdPaginatedAsync(int budgetId, PaginationParameters parameters)
    {
        var query = await _unitOfWork.Expenses.GetByBudgetIdAsync(budgetId);
        var expenses = query.ToList();

        // Aplicar búsqueda si se especifica
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            expenses = expenses.Where(e => 
                e.Description?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                e.FamilyMember?.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                e.BudgetCategory?.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            expenses = parameters.SortBy.ToLower() switch
            {
                "amount" => parameters.SortDirection?.ToLower() == "desc" 
                    ? expenses.OrderByDescending(e => e.Amount).ToList()
                    : expenses.OrderBy(e => e.Amount).ToList(),
                "date" => parameters.SortDirection?.ToLower() == "desc"
                    ? expenses.OrderByDescending(e => e.Date).ToList()
                    : expenses.OrderBy(e => e.Date).ToList(),
                "createdat" => parameters.SortDirection?.ToLower() == "desc"
                    ? expenses.OrderByDescending(e => e.CreatedAt).ToList()
                    : expenses.OrderBy(e => e.CreatedAt).ToList(),
                "description" => parameters.SortDirection?.ToLower() == "desc"
                    ? expenses.OrderByDescending(e => e.Description).ToList()
                    : expenses.OrderBy(e => e.Description).ToList(),
                "familymember" => parameters.SortDirection?.ToLower() == "desc"
                    ? expenses.OrderByDescending(e => e.FamilyMember?.Name).ToList()
                    : expenses.OrderBy(e => e.FamilyMember?.Name).ToList(),
                "category" => parameters.SortDirection?.ToLower() == "desc"
                    ? expenses.OrderByDescending(e => e.BudgetCategory?.Name).ToList()
                    : expenses.OrderBy(e => e.BudgetCategory?.Name).ToList(),
                _ => expenses.OrderBy(e => e.Date).ToList()
            };
        }
        else
        {
            expenses = expenses.OrderBy(e => e.Date).ToList();
        }

        var totalCount = expenses.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var currentPage = parameters.PageNumber;
        var pageSize = parameters.PageSize;

        var pagedData = expenses
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var expenseDtos = _mapper.Map<IEnumerable<ExpenseDto>>(pagedData);

        return new PaginatedResponse<ExpenseDto>
        {
            Data = expenseDtos,
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

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(id);
        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto> CreateAsync(int budgetId, CreateExpenseDto createExpenseDto)
    {
        // Verificar que el presupuesto existe
        var budget = await _unitOfWork.Budgets.GetByIdAsync(budgetId);
        if (budget == null)
            throw new ArgumentException($"No se encontró el presupuesto con ID {budgetId}");

        // Verificar que el miembro de la familia existe
        var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(createExpenseDto.FamilyMemberId);
        if (familyMember == null)
            throw new ArgumentException($"No se encontró el miembro de la familia con ID {createExpenseDto.FamilyMemberId}");

        // Verificar que la categoría existe y pertenece al presupuesto
        var category = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(createExpenseDto.BudgetCategoryId);
        if (category == null)
            throw new ArgumentException($"No se encontró la categoría con ID {createExpenseDto.BudgetCategoryId}");

        if (category.BudgetId != budgetId)
            throw new ArgumentException($"La categoría no pertenece al presupuesto especificado");

        // REGLA DE NEGOCIO CRÍTICA: No permitir gastos que excedan el límite de la categoría
        var currentSpent = await _unitOfWork.Expenses.GetTotalSpentByCategoryAsync(createExpenseDto.BudgetCategoryId);
        var remainingAmount = category.Limit - currentSpent;

        if (createExpenseDto.Amount > remainingAmount)
        {
            throw new CategoryLimitExceededException(
                category.Name, 
                currentSpent, 
                category.Limit, 
                createExpenseDto.Amount);
        }

        var expense = new Expense
        {
            Amount = createExpenseDto.Amount,
            Description = createExpenseDto.Description.Trim(),
            BudgetCategoryId = createExpenseDto.BudgetCategoryId,
            FamilyMemberId = createExpenseDto.FamilyMemberId,
            MonthId = budget.MonthId,
            Date = createExpenseDto.ExpenseDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Expenses.AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        var createdExpense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id);
        return _mapper.Map<ExpenseDto>(createdExpense);
    }

    public async Task DeleteAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            throw new ArgumentException($"No se encontró el gasto con ID {id}");

        await _unitOfWork.Expenses.DeleteAsync(expense);
        await _unitOfWork.SaveChangesAsync();
    }
} 