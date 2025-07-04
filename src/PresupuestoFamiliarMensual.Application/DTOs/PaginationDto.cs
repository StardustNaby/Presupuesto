namespace PresupuestoFamiliarMensual.Application.DTOs;

/// <summary>
/// Parámetros de paginación para las consultas
/// </summary>
public class PaginationParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    /// <summary>
    /// Número de página (comienza en 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Tamaño de la página (máximo 50)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// Término de búsqueda
    /// </summary>
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Respuesta paginada genérica
/// </summary>
/// <typeparam name="T">Tipo de datos</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Datos de la página actual
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Información de paginación
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();

    /// <summary>
    /// Total de elementos
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Página actual
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Tamaño de la página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// Información de paginación
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Total de elementos
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Página actual
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Tamaño de la página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage { get; set; }
} 