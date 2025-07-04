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
        Console.WriteLine("🔄 Creando 100 miembros de familia...");
        var familyMembers = new List<FamilyMember>();
        var nombres = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Carmen", "Pedro", "Isabel", "Miguel", "Rosa", "Jorge", "Patricia", "Fernando", "Lucía", "Roberto", "Elena", "Diego", "Sofia", "Alejandro", "Valeria" };
        var apellidos = new[] { "Pérez", "García", "López", "Martínez", "González", "Rodríguez", "Fernández", "Moreno", "Jiménez", "Torres", "Ruiz", "Hernández", "Díaz", "Sánchez", "Romero", "Alonso", "Gutiérrez", "Navarro", "Morales", "Molina" };
        
        for (int i = 1; i <= 100; i++)
        {
            var nombre = nombres[i % nombres.Length];
            var apellido = apellidos[i % apellidos.Length];
            familyMembers.Add(new FamilyMember 
            { 
                Name = $"{nombre} {apellido} {i}", 
                Email = $"{nombre.ToLower()}.{apellido.ToLower()}{i}@example.com" 
            });
        }

        context.FamilyMembers.AddRange(familyMembers);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {familyMembers.Count} miembros de familia creados");

        Console.WriteLine("🔄 Creando 100 meses (múltiples años)...");
        var months = new List<Month>();
        var nombresMeses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        
        for (int year = 2020; year <= 2027; year++)
        {
            for (int month = 1; month <= 12; month++)
            {
                months.Add(new Month 
                { 
                    Year = year, 
                    MonthNumber = month, 
                    Name = nombresMeses[month - 1] 
                });
                
                if (months.Count >= 100) break;
            }
            if (months.Count >= 100) break;
        }

        context.Months.AddRange(months);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {months.Count} meses creados");

        Console.WriteLine("🔄 Creando 100 presupuestos...");
        var budgets = new List<Budget>();
        var random = new Random(42); // Semilla fija para resultados consistentes
        
        for (int i = 1; i <= 100; i++)
        {
            var familyMemberId = (i % familyMembers.Count) + 1;
            var monthId = (i % months.Count) + 1;
            var totalAmount = random.Next(2000, 10000); // Entre 2000 y 10000
            
            budgets.Add(new Budget 
            { 
                FamilyMemberId = familyMemberId, 
                MonthId = monthId, 
                TotalAmount = totalAmount 
            });
        }

        context.Budgets.AddRange(budgets);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {budgets.Count} presupuestos creados");

        Console.WriteLine("🔄 Creando 100 categorías de presupuesto...");
        var categories = new List<BudgetCategory>();
        var nombresCategorias = new[] { "Alimentación", "Transporte", "Entretenimiento", "Servicios", "Salud", "Educación", "Vestimenta", "Hogar", "Tecnología", "Deportes", "Viajes", "Regalos", "Mascotas", "Jardín", "Arte", "Música", "Libros", "Cine", "Restaurantes", "Café" };
        
        for (int i = 1; i <= 100; i++)
        {
            var budgetId = (i % budgets.Count) + 1;
            var nombreCategoria = nombresCategorias[i % nombresCategorias.Length];
            var limit = random.Next(500, 3000); // Entre 500 y 3000
            
            categories.Add(new BudgetCategory 
            { 
                Name = $"{nombreCategoria} {i}", 
                Limit = limit, 
                BudgetId = budgetId 
            });
        }

        context.BudgetCategories.AddRange(categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {categories.Count} categorías creadas");

        Console.WriteLine("🔄 Creando 100 gastos...");
        var expenses = new List<Expense>();
        var descripciones = new[] { "Compras del supermercado", "Gasolina", "Cine", "Luz", "Comida rápida", "Medicamentos", "Libros", "Ropa", "Mantenimiento", "Internet", "Agua", "Gas", "Seguro", "Gimnasio", "Peluquería", "Dentista", "Óptica", "Farmacia", "Limpieza", "Reparaciones" };
        
        for (int i = 1; i <= 100; i++)
        {
            var categoryId = (i % categories.Count) + 1;
            var familyMemberId = (i % familyMembers.Count) + 1;
            var monthId = (i % months.Count) + 1;
            var descripcion = descripciones[i % descripciones.Length];
            var amount = random.Next(50, 1000); // Entre 50 y 1000
            
            expenses.Add(new Expense 
            { 
                Description = $"{descripcion} {i}", 
                Amount = amount, 
                BudgetCategoryId = categoryId, 
                FamilyMemberId = familyMemberId, 
                MonthId = monthId 
            });
        }

        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {expenses.Count} gastos creados");

        Console.WriteLine($"🎉 ¡Datos de ejemplo creados exitosamente!");
        Console.WriteLine($"📊 Resumen: {familyMembers.Count} miembros, {months.Count} meses, {budgets.Count} presupuestos, {categories.Count} categorías, {expenses.Count} gastos");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error creando datos de ejemplo: {ex.Message}");
        Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
    }
} 