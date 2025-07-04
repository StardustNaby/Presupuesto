namespace PresupuestoFamiliarMensual.Core.Entities;

/// <summary>
/// Entidad que representa un usuario del sistema
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Relaciones
    public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
} 