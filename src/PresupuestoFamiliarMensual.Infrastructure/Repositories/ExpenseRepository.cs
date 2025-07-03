using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Core.Entities;
using PresupuestoFamiliarMensual.Core.Interfaces;
using PresupuestoFamiliarMensual.Infrastructure.Data;

namespace PresupuestoFamiliarMensual.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de gastos
/// </summary>
public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Budget)
            .Include(e => e.FamilyMember)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Expense>> GetByBudgetIdAsync(int budgetId)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.FamilyMember)
            .Where(e => e.BudgetId == budgetId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Expenses
            .Include(e => e.FamilyMember)
            .Where(e => e.BudgetCategoryId == categoryId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSpentByCategoryAsync(int categoryId)
    {
        return await _context.Expenses
            .Where(e => e.BudgetCategoryId == categoryId)
            .SumAsync(e => e.Amount);
    }

    public async Task<decimal> GetTotalSpentByBudgetAsync(int budgetId)
    {
        return await _context.Expenses
            .Where(e => e.BudgetId == budgetId)
            .SumAsync(e => e.Amount);
    }
} 