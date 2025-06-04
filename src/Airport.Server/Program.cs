using Airport.Data;
using Airport.Hubs;
using Airport.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite DB context
builder.Services.AddDbContext<AirportDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// SignalR aa nemev
builder.Services.AddControllers();
builder.Services.AddSignalR();

// WebSocketServerService Hosted Service-aar nemeh
builder.Services.AddHostedService<WebSocketServerService>();

//  compress(hurd nemne)
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// DB uusgeh, seed hiih process
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
    endpoints.MapHub<StatusHub>("/statushub");
});

app.Run();
