using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;

namespace DataBase.Services
{
    public class RoomAvailabilityUpdater : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public RoomAvailabilityUpdater(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await UpdateRoomAvailability(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async Task UpdateRoomAvailability()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HotelDataBaseContext>();
                var today = DateTime.Now;

                try
                { 
                    var reservations = await context.Reservation.ToListAsync();
                    var rooms = await context.Rooms.ToListAsync();

                    foreach (var room in rooms)
                    {
                        var isBooked = reservations.Any(r =>
                            r.RoomId == room.RoomId &&
                            r.CheckInDate <= today &&
                            r.CheckOutDate >= today);

                        if (room.IsAvailable != !isBooked)
                        {
                            room.IsAvailable = !isBooked;
                        }
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка оновлення доступності кімнат: {ex.Message}");
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}