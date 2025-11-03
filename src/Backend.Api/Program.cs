using Backend.Application.Interfaces;
using Backend.Application.Services;
using Backend.Infrastructure.External;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var swaggerSection = builder.Configuration.GetSection("Swagger");
    var title = swaggerSection.GetValue<string>("Title") ?? "API";
    var version = swaggerSection.GetValue<string>("Version") ?? "v1";
    var description = swaggerSection.GetValue<string>("Description") ?? "Currency Conversion API";

    options.SwaggerDoc(version, new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = title,
        Version = version,
        Description = description
    });
});

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Use SQLite by default; you can switch to SQL Server easily
    options.UseSqlServer(conn);
});

builder.Services.AddScoped<IConversionRepository, ConversionRepository>();
builder.Services.AddScoped<IRateProvider, OpenExchangeRateProvider>();
builder.Services.AddScoped<ConvertCurrencyService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();


var rlSection = builder.Configuration.GetSection("RateLimiting");
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = rlSection.GetValue<int>("PermitLimit", 100);
        options.Window = TimeSpan.FromSeconds(rlSection.GetValue<int>("WindowSeconds", 60));
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = rlSection.GetValue<int>("QueueLimit", 10);
    }));


var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
