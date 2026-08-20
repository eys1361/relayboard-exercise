using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RelayBoard")
    ?? "Data Source=relayboard.db";

builder.Services.AddDbContext<RelayBoardContext>(options => options.UseSqlite(connectionString));
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

public partial class Program;
