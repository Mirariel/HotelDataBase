using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DataBase.Extensions;

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
        public IActionResult FindReservations(string passportNumber)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.PassportNumber == passportNumber);
            if (customer == null)
            {
                return this.ViewWithTempError($"Клієнта з номером паспорта {passportNumber} не знайдено.", "Search");
            }

            var reservations = _context.Reservation
                .Where(r => r.CustomerId == customer.CustomerId)
                .Include(r=>r.Room)
                .ThenInclude(r=>r.RoomType)
                .OrderBy(r => r.CheckInDate)
                .ToList();

            ViewBag.Customer = customer;
            ViewBag.Today = DateTime.Now;

            return View("ReservationList", reservations);
        }


        public IActionResult ReservationDetails(int reservationId)
        {
            var reservation = _context.Reservation
                .Include(r=>r.Room)
                .ThenInclude(r=>r.RoomType)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                return this.ViewWithTempError("Резервацію не знайдено.", "Search");
            }

            var services = _context.ServiceUsage
                .Where(su => su.ReservationId == reservationId)
                .Select(su => new
                {
                    su.Services.ServicesName,
                    su.ExecutionDate,
                    su.Services.Price
                })
                .ToList();

            ViewBag.Reservation = reservation;
            return View(services);
        }
    }
}
