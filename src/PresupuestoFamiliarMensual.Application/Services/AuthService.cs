using AutoMapper;
using BCrypt.Net;
using PresupuestoFamiliarMensual.Application.DTOs;
using PresupuestoFamiliarMensual.Core.Entities;
using PresupuestoFamiliarMensual.Core.Interfaces;
using System.Security.Claims;

namespace PresupuestoFamiliarMensual.Application.Services;

/// <summary>
/// Implementación del servicio de autenticación
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly Dictionary<string, DateTime> _refreshTokens = new();

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto)
    {
        // Verificar que el username no exista
        if (await _unitOfWork.Users.ExistsByUsernameAsync(registerDto.Username))
            throw new InvalidOperationException("El nombre de usuario ya está en uso");

        // Verificar que el email no exista
        if (await _unitOfWork.Users.ExistsByEmailAsync(registerDto.Email))
            throw new InvalidOperationException("El email ya está registrado");

        // Crear el usuario
        var user = new User
        {
            Username = registerDto.Username.Trim(),
            Email = registerDto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60); // 1 hora

        // Almacenar refresh token (en producción usar Redis o base de datos)
        _refreshTokens[refreshToken] = _jwtService.GetRefreshTokenExpiration();

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        // Buscar usuario por username
        var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
        if (user == null)
            throw new InvalidOperationException("Credenciales inválidas");

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new InvalidOperationException("Credenciales inválidas");

        // Verificar que el usuario esté activo
        if (!user.IsActive)
            throw new InvalidOperationException("La cuenta está deshabilitada");

        // Actualizar último login
        await _unitOfWork.Users.UpdateLastLoginAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60); // 1 hora

        // Almacenar refresh token
        _refreshTokens[refreshToken] = _jwtService.GetRefreshTokenExpiration();

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Verificar que el refresh token exista y no haya expirado
        if (!_refreshTokens.TryGetValue(refreshToken, out var expiration) || expiration < DateTime.UtcNow)
            throw new InvalidOperationException("Token de refresco inválido o expirado");

        // Obtener el usuario del token expirado (en producción, almacenar el userId con el refresh token)
        // Por simplicidad, aquí asumimos que el refresh token está asociado al usuario
        // En una implementación real, deberías almacenar el userId junto con el refresh token

        // Remover el refresh token usado
        _refreshTokens.Remove(refreshToken);

        throw new NotImplementedException("Implementación completa requiere almacenamiento de refresh tokens");
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Usuario no encontrado");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Usuario no encontrado");

        // Verificar que el email no esté en uso por otro usuario
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(updateDto.Email);
        if (existingUser != null && existingUser.Id != userId)
            throw new InvalidOperationException("El email ya está en uso por otro usuario");

        // Actualizar datos
        user.FirstName = updateDto.FirstName.Trim();
        user.LastName = updateDto.LastName.Trim();
        user.Email = updateDto.Email.Trim().ToLower();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("Usuario no encontrado");

        // Verificar contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("La contraseña actual es incorrecta");

        // Actualizar contraseña
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        return _refreshTokens.Remove(refreshToken);
    }
} 