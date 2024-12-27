using DataBase.Models;
using DataBase.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HotelDataBaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHostedService<RoomAvailabilityUpdater>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "customersRoute",
        pattern: "Customers/{action=Index}/{id?}",
        defaults: new { controller = "Customers" });

    endpoints.MapControllerRoute(
    name: "EmployeesRoute",
    pattern: "Employees/{action=Index}/{id?}",
    defaults: new { controller = "Employees" });

    endpoints.MapControllerRoute(
name: "ReservationsRoute",
pattern: "Reservations/{action=Index}/{id?}",
defaults: new { controller = "Reservations" });
    
    endpoints.MapControllerRoute(
name: "RoomsRoute",
pattern: "Rooms/{action=Index}/{id?}",
defaults: new { controller = "Rooms" });
    
    endpoints.MapControllerRoute(
name: "ServiceUsagesRoute",
pattern: "ServiceUsages/{action=Index}/{id?}",
defaults: new { controller = "ServiceUsages" });

    endpoints.MapControllerRoute(
        name: "HotelRoomRoute",
        pattern: "HotelRoom/{action=Index}/{id?}",
        defaults: new { controller = "HotelRoom" });
});


app.Run();
