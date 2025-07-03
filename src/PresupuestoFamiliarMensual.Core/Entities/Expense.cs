using System.ComponentModel.DataAnnotations;

namespace PresupuestoFamiliarMensual.Core.Entities;

/// <summary>
/// Representa un gasto registrado con monto y categoría
/// </summary>
public class Expense
{
    public int Id { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public int BudgetCategoryId { get; set; }
    
    [Required]
    public int BudgetId { get; set; }
    
    [Required]
    public int FamilyMemberId { get; set; }
    
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public virtual BudgetCategory Category { get; set; } = null!;
    public virtual Budget Budget { get; set; } = null!;
    public virtual FamilyMember FamilyMember { get; set; } = null!;
} 