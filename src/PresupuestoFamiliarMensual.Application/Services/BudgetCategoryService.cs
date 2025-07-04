using AutoMapper;
using PresupuestoFamiliarMensual.Application.DTOs;
using PresupuestoFamiliarMensual.Core.Entities;
using PresupuestoFamiliarMensual.Core.Exceptions;
using PresupuestoFamiliarMensual.Core.Interfaces;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Implementación del servicio de categorías de presupuesto
/// </summary>
public class BudgetCategoryService : IBudgetCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BudgetCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BudgetCategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.BudgetCategories.GetAllAsync();
        return _mapper.Map<IEnumerable<BudgetCategoryDto>>(categories);
    }

    public async Task<PaginatedResponse<BudgetCategoryDto>> GetPaginatedAsync(PaginationParameters parameters)
    {
        var query = await _unitOfWork.BudgetCategories.GetAllAsync();
        var categories = query.ToList();

        // Aplicar búsqueda si se especifica
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            categories = categories.Where(c => 
                c.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            categories = parameters.SortBy.ToLower() switch
            {
                "name" => parameters.SortDirection?.ToLower() == "desc" 
                    ? categories.OrderByDescending(c => c.Name).ToList()
                    : categories.OrderBy(c => c.Name).ToList(),
                "limit" => parameters.SortDirection?.ToLower() == "desc"
                    ? categories.OrderByDescending(c => c.Limit).ToList()
                    : categories.OrderBy(c => c.Limit).ToList(),
                "createdat" => parameters.SortDirection?.ToLower() == "desc"
                    ? categories.OrderByDescending(c => c.CreatedAt).ToList()
                    : categories.OrderBy(c => c.CreatedAt).ToList(),
                _ => categories.OrderBy(c => c.Name).ToList()
            };
        }
        else
        {
            categories = categories.OrderBy(c => c.Name).ToList();
        }

        var totalCount = categories.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var currentPage = parameters.PageNumber;
        var pageSize = parameters.PageSize;

        var pagedData = categories
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var categoryDtos = _mapper.Map<IEnumerable<BudgetCategoryDto>>(pagedData);

        return new PaginatedResponse<BudgetCategoryDto>
        {
            Data = categoryDtos,
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

    public async Task<IEnumerable<BudgetCategoryDto>> GetByBudgetIdAsync(int budgetId)
    {
        var categories = await _unitOfWork.BudgetCategories.GetByBudgetIdAsync(budgetId);
        return _mapper.Map<IEnumerable<BudgetCategoryDto>>(categories);
    }

    public async Task<PaginatedResponse<BudgetCategoryDto>> GetByBudgetIdPaginatedAsync(int budgetId, PaginationParameters parameters)
    {
        var query = await _unitOfWork.BudgetCategories.GetByBudgetIdAsync(budgetId);
        var categories = query.ToList();

        // Aplicar búsqueda si se especifica
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            categories = categories.Where(c => 
                c.Name?.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            categories = parameters.SortBy.ToLower() switch
            {
                "name" => parameters.SortDirection?.ToLower() == "desc" 
                    ? categories.OrderByDescending(c => c.Name).ToList()
                    : categories.OrderBy(c => c.Name).ToList(),
                "limit" => parameters.SortDirection?.ToLower() == "desc"
                    ? categories.OrderByDescending(c => c.Limit).ToList()
                    : categories.OrderBy(c => c.Limit).ToList(),
                "createdat" => parameters.SortDirection?.ToLower() == "desc"
                    ? categories.OrderByDescending(c => c.CreatedAt).ToList()
                    : categories.OrderBy(c => c.CreatedAt).ToList(),
                _ => categories.OrderBy(c => c.Name).ToList()
            };
        }
        else
        {
            categories = categories.OrderBy(c => c.Name).ToList();
        }

        var totalCount = categories.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var currentPage = parameters.PageNumber;
        var pageSize = parameters.PageSize;

        var pagedData = categories
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var categoryDtos = _mapper.Map<IEnumerable<BudgetCategoryDto>>(pagedData);

        return new PaginatedResponse<BudgetCategoryDto>
        {
            Data = categoryDtos,
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

    public async Task<BudgetCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(id);
        return _mapper.Map<BudgetCategoryDto>(category);
    }

    public async Task<BudgetCategoryDto> CreateAsync(CreateBudgetCategoryDto createCategoryDto)
    {
        // Verificar que el presupuesto existe
        var budget = await _unitOfWork.Budgets.GetByIdAsync(createCategoryDto.BudgetId);
        if (budget == null)
            throw new ArgumentException($"No se encontró el presupuesto con ID {createCategoryDto.BudgetId}");

        // REGLA DE NEGOCIO: No permitir categorías con nombre repetido
        var normalizedName = createCategoryDto.Name.Trim();
        var existsByName = await _unitOfWork.BudgetCategories.ExistsByNameInBudgetAsync(normalizedName, createCategoryDto.BudgetId);
        if (existsByName)
            throw new DuplicateCategoryNameException(normalizedName, createCategoryDto.BudgetId);

        var category = new BudgetCategory
        {
            Name = normalizedName,
            Limit = createCategoryDto.Limit,
            BudgetId = createCategoryDto.BudgetId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BudgetCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var createdCategory = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(category.Id);
        return _mapper.Map<BudgetCategoryDto>(createdCategory);
    }

    public async Task<BudgetCategoryDto> UpdateAsync(int id, UpdateBudgetCategoryDto updateCategoryDto)
    {
        var category = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(id);
        if (category == null)
            throw new ArgumentException($"No se encontró la categoría con ID {id}");

        // REGLA DE NEGOCIO: No permitir categorías con nombre repetido (excluyendo la actual)
        var normalizedName = updateCategoryDto.Name.Trim();
        var existsByName = await _unitOfWork.BudgetCategories.ExistsByNameInBudgetAsync(normalizedName, category.BudgetId, id);
        if (existsByName)
            throw new DuplicateCategoryNameException(normalizedName, category.BudgetId);

        category.Name = normalizedName;
        category.Limit = updateCategoryDto.Limit;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BudgetCategories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var updatedCategory = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(category.Id);
        return _mapper.Map<BudgetCategoryDto>(updatedCategory);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _unitOfWork.BudgetCategories.GetByIdWithExpensesAsync(id);
        if (category == null)
            throw new ArgumentException($"No se encontró la categoría con ID {id}");

        // REGLA DE NEGOCIO: No permitir eliminar categorías con gastos registrados
        if (category.HasExpenses)
            throw new CategoryWithExpensesException(category.Name, category.Id, category.Expenses.Count);

        await _unitOfWork.BudgetCategories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();
    }
} 