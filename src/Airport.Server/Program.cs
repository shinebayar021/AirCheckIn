using Airport.Data;
using Airport.Hubs;
using Airport.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Airport.Business.Services;
using Airport.Business.FlightServices.Interfaces;
using Airport.Business.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// CORS policy for Kiosk client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowKioskClient", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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

builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

app.UseCors("AllowKioskClient");

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
