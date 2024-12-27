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
            // Запускаємо таймер одразу після старту сервісу
            _timer = new Timer(UpdateRoomAvailability, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        private async void UpdateRoomAvailability(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HotelDataBaseContext>();
                var today = DateTime.Today;

                try
                {
                    // Додавання тестових даних для перевірки
                    await AddTestData(context);

                    // Завантаження даних
                    var reservations = await context.Reservations.ToListAsync();
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
                    // Логування помилок
                    Console.WriteLine($"Помилка оновлення доступності кімнат: {ex.Message}");
                }
            }
        }

        private async Task AddTestData(HotelDataBaseContext context)
        {
            // Додаємо тестові кімнати, якщо їх немає
            if (!await context.Rooms.AnyAsync())
            {
                context.Rooms.AddRange(
                new Room
                {
                   
                    IsAvailable = true,
                    RoomNumber = 102,
                    Capacity = 2, 
                    Description = "skgnsoidgbsdi", 
                    Price = 1200,
                    RoomType = "Deluxe"
                  
                });
            }

            // Додаємо тестові бронювання, якщо їх немає
            if (!await context.Reservations.AnyAsync())
            {
                context.Reservations.Add(new Reservation
                {
                    ReservationId = 1,
                    RoomId = 1,
                    CheckInDate = DateTime.Today.AddDays(-1),
                    CheckOutDate = DateTime.Today.AddDays(1)
                });
            }

            // Зберігаємо тестові дані
            await context.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Зупиняємо таймер, якщо сервіс завершує роботу
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Звільняємо ресурси таймера
            _timer?.Dispose();
        }
    }
}