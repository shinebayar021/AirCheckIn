using Airport.Data;
using Airport.Hubs;
using Airport.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite DB context бүртгэх
builder.Services.AddDbContext<AirportDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Контроллер болон SignalR нэмэх
builder.Services.AddControllers();
builder.Services.AddSignalR();

// WebSocketServerService-ийг Hosted Service болгон бүртгэнэ
builder.Services.AddHostedService<WebSocketServerService>();

//  Хэрэв хэрэгтэй бол SignalR-д зориулж compression
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// DB үүсгэх, seed хийх
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AirportDbContext>();
    db.Database.EnsureCreated();
    db.SeedData();
}

//  Middleware
app.UseRouting();
app.UseResponseCompression();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // SignalR Hub энд бүртгэгдэж байна
    endpoints.MapHub<StatusHub>("/statushub");
});

app.Run();
