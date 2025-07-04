using PresupuestoFamiliarMensual.Application.DTOs;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Interfaz para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    /// <param name="registerDto">Datos de registro</param>
    /// <returns>Respuesta de autenticación</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto);

    /// <summary>
    /// Autentica un usuario
    /// </summary>
    /// <param name="loginDto">Datos de login</param>
    /// <returns>Respuesta de autenticación</returns>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);

    /// <summary>
    /// Refresca el token de acceso
    /// </summary>
    /// <param name="refreshToken">Token de refresco</param>
    /// <returns>Nueva respuesta de autenticación</returns>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Obtiene el perfil del usuario actual
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Información del usuario</returns>
    Task<UserDto> GetProfileAsync(int userId);

    /// <summary>
    /// Actualiza el perfil del usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="updateDto">Datos de actualización</param>
    /// <returns>Usuario actualizado</returns>
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);

    /// <summary>
    /// Cambia la contraseña del usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="changePasswordDto">Datos de cambio de contraseña</param>
    /// <returns>True si se cambió correctamente</returns>
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Revoca el token de refresco
    /// </summary>
    /// <param name="refreshToken">Token de refresco</param>
    /// <returns>True si se revocó correctamente</returns>
    Task<bool> RevokeTokenAsync(string refreshToken);
} 