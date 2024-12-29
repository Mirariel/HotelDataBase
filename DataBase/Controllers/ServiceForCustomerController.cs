using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using System.Linq;

namespace DataBase.Controllers
{
    public class ServiceForCustomerController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ServiceForCustomerController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Index(int customerId)
        {
            var today = DateTime.Today;

            var hasBookingToday = _context.Reservation
                .Any(b => b.CustomerId == customerId && b.CheckInDate <= today && b.CheckOutDate>=today);

            if (!hasBookingToday)
            {
                ViewBag.ErrorMessage = "У вас немає бронювань на сьогодні.";
                return View("NoBooking");
            }

            // Отримуємо список доступних послуг
            var services = _context.Services.ToList();

            return View(services);
        }

        [HttpPost]
        public IActionResult UseService(int serviceId, int reservationId)
        {
            var usage = new ServiceUsage
            {
                ServicesId = serviceId,
                ReservationId = reservationId,
                ExecutionDate = DateTime.Now
            };

            _context.ServiceUsage.Add(usage);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}

