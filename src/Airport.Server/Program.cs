using Airport.Data; // AirportDbContext байх ёстой
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// appsettings.json дахь холболтын мөрийг ашиглаж DbContext бүртгэх
builder.Services.AddDbContext<AirportDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// **Контроллер нэмэх хэрэгтэй**
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AirportDbContext>();
    db.Database.EnsureCreated();
    db.SeedData();
}

// **Routing болон контроллерийг холбох тохиргоо**
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
