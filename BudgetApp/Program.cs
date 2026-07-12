using BudgetApp.Data;
using BudgetApp.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/budgetapp-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting App");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/budgetapp-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddRazorPages();

    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opts =>
    {
        opts.IdleTimeout = TimeSpan.FromHours(8);
        opts.Cookie.HttpOnly = true;
        opts.Cookie.IsEssential = true;
    });

    builder.Services.AddHttpContextAccessor();

    RegisterAppServices(builder.Services);

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    if (!IsRunningInContainer())
    {
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
    app.UseAuthorization();
    app.MapRazorPages();

    await ApplyMigrationsAsync(app);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Budget App terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void RegisterAppServices(IServiceCollection services)
{
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IItemNameService, ItemNameService>();
    services.AddScoped<IBudgetService, BudgetService>();
    services.AddScoped<IReportService, ReportService>();
}

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

static bool IsRunningInContainer()
{
    return string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);
}
