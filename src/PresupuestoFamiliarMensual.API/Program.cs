using Microsoft.EntityFrameworkCore;
using PresupuestoFamiliarMensual.Infrastructure.Data;
using PresupuestoFamiliarMensual.Infrastructure.Repositories;
using PresupuestoFamiliarMensual.Core.Interfaces;
using PresupuestoFamiliarMensual.Application.Services;
using AutoMapper;
using PresupuestoFamiliarMensual.Application.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Configuración básica para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Puerto: {port}");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configuración de base de datos (temporalmente deshabilitada)
Console.WriteLine("⚠️ Base de datos temporalmente deshabilitada para diagnóstico");

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Repositories (temporalmente comentados)
// builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
// builder.Services.AddScoped<IBudgetCategoryRepository, BudgetCategoryRepository>();
// builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
// builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Unit of Work (temporalmente comentado)
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services (temporalmente comentados)
// builder.Services.AddScoped<IBudgetService, BudgetService>();
// builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
// builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Servicios mínimos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

Console.WriteLine("🚀 Aplicación iniciada");

// Migraciones temporalmente deshabilitadas
Console.WriteLine("⚠️ Migraciones temporalmente deshabilitadas para diagnóstico");

// Pipeline mínimo
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("✅ Aplicación lista");
app.Run(); 