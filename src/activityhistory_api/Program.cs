using activityhistory_application.Commands.Commands;
using activityhistory_application.Interfaces;
using activityhistory_domain.Interfaces;
using activityhistory_infrastructure.Persistence.Context;
using activityhistory_infrastructure.Persistence.Repositories;
using activityhistory_infrastructure.Services;
using Aplicaactivityhistory_applicationtion.Queries.Handlers;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


//crear variable para la cadena de conexion
var connectionString = builder.Configuration.GetConnectionString("ConnectionPostgre"); //ConnectionPostgre es el parametro de conexion que creamos en el appsetting
//registrar servicio para la conexion


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("activityhistory_infrastructure")));

builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7181/api/users/");
});


//Inyeccion de dependencias
builder.Services.AddScoped<IActivityHistoryRepositoryPostgres, ActivityHistoryRepositoryPostgres>();
builder.Services.AddScoped<IUserServices, UserService>();

// MediatR Configuration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateActivityCommand).Assembly,
    typeof(GetActivitiesByUserEmailHandler).Assembly));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowLocalhost3000");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Obtiene el DbContext
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones a la base de datos.");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
