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
    Console.WriteLine("🔗 Configurando base de datos...");
    
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
    Console.WriteLine("⚠️ DATABASE_URL no encontrado - modo sin base de datos");
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
            new FamilyMember { Name = "Juan Pérez", Age = 35, Role = "Padre" },
            new FamilyMember { Name = "María Pérez", Age = 32, Role = "Madre" },
            new FamilyMember { Name = "Carlos Pérez", Age = 12, Role = "Hijo" },
            new FamilyMember { Name = "Ana Pérez", Age = 8, Role = "Hija" }
        };

        context.FamilyMembers.AddRange(familyMembers);
        await context.SaveChangesAsync();

        // Crear meses
        var months = new[]
        {
            new Month { Name = "Enero", Number = 1 },
            new Month { Name = "Febrero", Number = 2 },
            new Month { Name = "Marzo", Number = 3 },
            new Month { Name = "Abril", Number = 4 },
            new Month { Name = "Mayo", Number = 5 },
            new Month { Name = "Junio", Number = 6 },
            new Month { Name = "Julio", Number = 7 },
            new Month { Name = "Agosto", Number = 8 },
            new Month { Name = "Septiembre", Number = 9 },
            new Month { Name = "Octubre", Number = 10 },
            new Month { Name = "Noviembre", Number = 11 },
            new Month { Name = "Diciembre", Number = 12 }
        };

        context.Months.AddRange(months);
        await context.SaveChangesAsync();

        // Crear presupuestos
        var budgets = new[]
        {
            new Budget { FamilyMemberId = 1, MonthId = 7, Year = 2025, TotalAmount = 5000 },
            new Budget { FamilyMemberId = 2, MonthId = 7, Year = 2025, TotalAmount = 3000 },
            new Budget { FamilyMemberId = 1, MonthId = 8, Year = 2025, TotalAmount = 5500 }
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
            new Expense { Description = "Compras del supermercado", Amount = 250, CategoryId = 1, BudgetId = 1 },
            new Expense { Description = "Gasolina", Amount = 150, CategoryId = 2, BudgetId = 1 },
            new Expense { Description = "Cine", Amount = 80, CategoryId = 3, BudgetId = 1 },
            new Expense { Description = "Luz", Amount = 200, CategoryId = 4, BudgetId = 1 },
            new Expense { Description = "Comida rápida", Amount = 120, CategoryId = 5, BudgetId = 2 }
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