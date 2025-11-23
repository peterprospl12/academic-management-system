using AMS.Application.Common.Interfaces;
using AMS.Application.Interfaces;
using AMS.Application.Services;
using AMS.ConsoleUI.Views;
using AMS.Infrastructure.Persistence;
using AMS.Infrastructure.Persistence.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Terminal.Gui;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IMasterStudentService, MasterStudentService>();
builder.Services.AddScoped<IProfessorService, ProfessorService>();
builder.Services.AddScoped<ISequenceService, SequenceService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IOfficeService, OfficeService>();

builder.Services.AddScoped<IDataSeederService, DataSeeder>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var seeder = services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas uruchamiania aplikacji: {ex.Message}");
    }
}

Application.Init();

var win = new MainView(app.Services);

Application.Top.Add(win);
Application.Run();

Application.Shutdown();