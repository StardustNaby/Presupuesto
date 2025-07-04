using PresupuestoFamiliarMensual.Core.Entities;

namespace PresupuestoFamiliarMensual.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de usuarios
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Obtiene un usuario por nombre de usuario
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Obtiene un usuario por email
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Verifica si existe un usuario con el nombre de usuario especificado
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>True si existe, false en caso contrario</returns>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    /// Verifica si existe un usuario con el email especificado
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <returns>True si existe, false en caso contrario</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Actualiza la fecha del último login
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>True si se actualizó correctamente</returns>
    Task<bool> UpdateLastLoginAsync(int userId);
} 