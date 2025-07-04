using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Infrastructure.Data;
using PresupuestoFamiliarMensual.Infrastructure.Repositories;
using PresupuestoFamiliarMensual.Core.Interfaces;
using PresupuestoFamiliarMensual.Application.Services;
using PresupuestoFamiliarMensual.Core.Entities;
using AutoMapper;
using PresupuestoFamiliarMensual.Application.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Configuración básica para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Puerto: {port}");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configuración de base de datos
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    Console.WriteLine("🔗 Configurando base de datos PostgreSQL...");
    
    // Convertir formato URL a formato de conexión
    if (dbUrl.StartsWith("postgres://") || dbUrl.StartsWith("postgresql://"))
    {
        var url = dbUrl.Replace("postgresql://", "postgres://");
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        Console.WriteLine("✅ Base de datos PostgreSQL configurada");
    }
    else
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dbUrl));
        Console.WriteLine("✅ Base de datos configurada con cadena directa");
    }
}
else
{
    Console.WriteLine("🔗 Configurando base de datos SQLite local...");
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    Console.WriteLine("✅ Base de datos SQLite configurada");
}

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Repositories
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBudgetCategoryRepository, BudgetCategoryRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Servicios mínimos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

Console.WriteLine("🚀 Aplicación iniciada");

// Ejecutar migraciones si hay base de datos
if (!string.IsNullOrEmpty(dbUrl))
{
    try
    {
        Console.WriteLine("🔄 Ejecutando migraciones...");
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Verificar conexión
        Console.WriteLine("🔍 Verificando conexión a la base de datos...");
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"📊 ¿Puede conectar a la BD? {canConnect}");
        
        if (canConnect)
        {
            Console.WriteLine("🔄 Aplicando migraciones...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Migraciones ejecutadas correctamente");
            
            // Verificar si hay datos y crear datos de ejemplo si está vacío
            if (!await context.Budgets.AnyAsync())
            {
                Console.WriteLine("📊 Base de datos vacía, creando datos de ejemplo...");
                await SeedDatabaseAsync(context);
                Console.WriteLine("✅ Datos de ejemplo creados correctamente");
            }
            else
            {
                Console.WriteLine("📊 Base de datos ya contiene datos");
            }
        }
        else
        {
            Console.WriteLine("❌ No se puede conectar a la base de datos");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error en migraciones: {ex.Message}");
        Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
    }
}
else
{
    Console.WriteLine("⚠️ No hay DATABASE_URL configurado");
}

// Pipeline mínimo
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("✅ Aplicación lista");
app.Run();

// Método para crear datos de ejemplo
static async Task SeedDatabaseAsync(ApplicationDbContext context)
{
    try
    {
        // Crear miembros de familia
        var familyMembers = new[]
        {
            new FamilyMember { Name = "Juan Pérez", Email = "juan@example.com" },
            new FamilyMember { Name = "María Pérez", Email = "maria@example.com" },
            new FamilyMember { Name = "Carlos Pérez", Email = "carlos@example.com" },
            new FamilyMember { Name = "Ana Pérez", Email = "ana@example.com" }
        };

        context.FamilyMembers.AddRange(familyMembers);
        await context.SaveChangesAsync();

        // Crear meses
        var months = new[]
        {
            new Month { Year = 2024, MonthNumber = 1, Name = "Enero" },
            new Month { Year = 2024, MonthNumber = 2, Name = "Febrero" },
            new Month { Year = 2024, MonthNumber = 3, Name = "Marzo" },
            new Month { Year = 2024, MonthNumber = 4, Name = "Abril" },
            new Month { Year = 2024, MonthNumber = 5, Name = "Mayo" },
            new Month { Year = 2024, MonthNumber = 6, Name = "Junio" },
            new Month { Year = 2024, MonthNumber = 7, Name = "Julio" },
            new Month { Year = 2024, MonthNumber = 8, Name = "Agosto" },
            new Month { Year = 2024, MonthNumber = 9, Name = "Septiembre" },
            new Month { Year = 2024, MonthNumber = 10, Name = "Octubre" },
            new Month { Year = 2024, MonthNumber = 11, Name = "Noviembre" },
            new Month { Year = 2024, MonthNumber = 12, Name = "Diciembre" }
        };

        context.Months.AddRange(months);
        await context.SaveChangesAsync();

        // Crear presupuestos
        var budgets = new[]
        {
            new Budget { FamilyMemberId = 1, MonthId = 7, TotalAmount = 5000 },
            new Budget { FamilyMemberId = 2, MonthId = 7, TotalAmount = 3000 },
            new Budget { FamilyMemberId = 1, MonthId = 8, TotalAmount = 5500 }
        };

        context.Budgets.AddRange(budgets);
        await context.SaveChangesAsync();

        // Crear categorías
        var categories = new[]
        {
            new BudgetCategory { Name = "Alimentación", Limit = 1500, BudgetId = 1 },
            new BudgetCategory { Name = "Transporte", Limit = 800, BudgetId = 1 },
            new BudgetCategory { Name = "Entretenimiento", Limit = 500, BudgetId = 1 },
            new BudgetCategory { Name = "Servicios", Limit = 1200, BudgetId = 1 },
            new BudgetCategory { Name = "Alimentación", Limit = 1000, BudgetId = 2 },
            new BudgetCategory { Name = "Transporte", Limit = 600, BudgetId = 2 }
        };

        context.BudgetCategories.AddRange(categories);
        await context.SaveChangesAsync();

        // Crear gastos
        var expenses = new[]
        {
            new Expense { Description = "Compras del supermercado", Amount = 250, BudgetCategoryId = 1, FamilyMemberId = 1, MonthId = 7 },
            new Expense { Description = "Gasolina", Amount = 150, BudgetCategoryId = 2, FamilyMemberId = 1, MonthId = 7 },
            new Expense { Description = "Cine", Amount = 80, BudgetCategoryId = 3, FamilyMemberId = 1, MonthId = 7 },
            new Expense { Description = "Luz", Amount = 200, BudgetCategoryId = 4, FamilyMemberId = 1, MonthId = 7 },
            new Expense { Description = "Comida rápida", Amount = 120, BudgetCategoryId = 5, FamilyMemberId = 2, MonthId = 7 }
        };

        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Datos creados: {familyMembers.Length} miembros, {budgets.Length} presupuestos, {categories.Length} categorías, {expenses.Length} gastos");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error creando datos de ejemplo: {ex.Message}");
    }
} 