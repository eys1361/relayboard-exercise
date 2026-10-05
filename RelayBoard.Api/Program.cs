using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RelayBoard")
    ?? "Data Source=relayboard.db";

builder.Services.AddDbContext<RelayBoardContext>(options => options.UseSqlite(connectionString));
var demoNow = builder.Configuration.GetValue<DateTimeOffset?>("Demo:Now");
builder.Services.AddSingleton<TimeProvider>(
    demoNow is { } now ? new FixedTimeProvider(now) : TimeProvider.System);
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RelayBoardContext>();
    await db.Database.EnsureCreatedAsync();
    await SchemaUpgrade.ApplyAsync(db);
    await SeedData.EnsureSeededAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();
app.Run();

public partial class Program
{
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
