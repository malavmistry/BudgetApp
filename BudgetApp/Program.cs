using BudgetApp.Data;
using BudgetApp.Services;
using Microsoft.Data.SqlClient;
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
    builder.Configuration
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .AddCommandLine(args);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/budgetapp-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddRazorPages();

    var connectionString = GetDatabaseConnectionString(builder.Configuration);
    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

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
    throw;
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
    services.AddScoped<IRecurringItemService, RecurringItemService>();
    services.AddScoped<IReportService, ReportService>();
}

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Applying pending database migrations");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations complete");
}

static string GetDatabaseConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    var host = configuration["Database:Host"];
    var database = configuration["Database:Name"];
    var user = configuration["Database:User"];
    var password = configuration["Database:Password"];
    var port = configuration.GetValue<int>("Database:Port");

    if (string.IsNullOrWhiteSpace(host) ||
        string.IsNullOrWhiteSpace(database) ||
        string.IsNullOrWhiteSpace(user) ||
        string.IsNullOrWhiteSpace(password) ||
        port is < 1 or > 65535)
    {
        throw new InvalidOperationException(
            "Configure Database:Host, Port (1-65535), Name, User, and Password " +
            "in appsettings.Local.json or environment variables, or supply ConnectionStrings:DefaultConnection.");
    }

    return new SqlConnectionStringBuilder
    {
        DataSource = $"{host},{port}",
        InitialCatalog = database,
        UserID = user,
        Password = password,
        Encrypt = true,
        TrustServerCertificate = true,
        MultipleActiveResultSets = true
    }.ConnectionString;
}

static bool IsRunningInContainer()
{
    return string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);
}
