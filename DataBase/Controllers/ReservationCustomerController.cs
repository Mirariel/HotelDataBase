using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using DataBase.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataBase.Controllers
{
    public class ReservationCustomerController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ReservationCustomerController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FindReservations(string passportNumber)
        {
            var customer = await GetCustomerByPassportAsync(passportNumber);
            if (customer == null)
                return this.ViewWithTempError($"Клієнта з номером паспорта {passportNumber} не знайдено.", "Search");

            var reservations = await GetReservationsByCustomerAsync(customer.CustomerId);

            ViewBag.Customer = customer;
            ViewBag.Today = DateTime.Now;

            return View("ReservationList", reservations);
        }

        public async Task<IActionResult> ReservationDetails(int reservationId)
        {
            var reservation = await GetReservationWithDetailsAsync(reservationId);
            if (reservation == null)
                return this.ViewWithTempError("Резервацію не знайдено.", "Search");

            var services = await GetServicesByReservationAsync(reservationId);

            ViewBag.Reservation = reservation;
            return View(services);
        }

        private async Task<Customer> GetCustomerByPassportAsync(string passportNumber)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.PassportNumber == passportNumber);
        }

        private async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerId)
        {
            return await _context.Reservation
                .Where(r => r.CustomerId == customerId)
                .Include(r => r.Room)
                .ThenInclude(r => r.RoomType)
                .OrderBy(r => r.CheckInDate)
                .ToListAsync();
        }

        private async Task<Reservation> GetReservationWithDetailsAsync(int reservationId)
        {
            return await _context.Reservation
                .Include(r => r.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }

        private async Task<List<object>> GetServicesByReservationAsync(int reservationId)
        {
            return await _context.ServiceUsage
                .Where(su => su.ReservationId == reservationId)
                .Select(su => new
                {
                    su.Services.ServicesName,
                    su.ExecutionDate,
                    su.Services.Price
                })
                .ToListAsync<object>();
        }
    }
}