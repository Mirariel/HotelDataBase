using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Controllers
{
    public class ServiceForCustomerController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ServiceForCustomerController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        [HttpPost]
        public IActionResult CheckReservation(string passportNumber, int serviceId)
        {
            var today = DateTime.Now;

            var customer = _context.Customers.FirstOrDefault(c => c.PassportNumber == passportNumber);
            if (customer == null)
            {
                TempData["ErrorMessage"] = $"Клієнта з номером паспорта {passportNumber} не знайдено.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("ServiceOrder", new { customerId = customer.CustomerId, serviceId = serviceId});
        }

        public IActionResult ServiceOrder(int customerId, int serviceId)
        {
            var service = _context.Services.FirstOrDefault(s => s.ServicesId == serviceId);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Послугу не знайдено.";
                return RedirectToAction("Index");
            }

            var reservations = _context.Reservation
                .Where(r => r.CustomerId == customerId && r.CheckOutDate >= DateTime.Today)
                .Include(r => r.Room)
                .ToList();

            if (!reservations.Any())
            {
                TempData["ErrorMessage"] = "У клієнта немає активних бронювань.";
                return RedirectToAction("Index");
            }

            ViewBag.Reservations = reservations;
            ViewBag.CustomerId = customerId;   
            ViewBag.ServiceId = serviceId;     

            return View(service);
        }


        [HttpPost]
        public IActionResult ConfirmServiceOrder(int serviceId, int customerId, int reservationId, DateTime selectedDate, string selectedTime)
        {
            var Date = DateTime.Parse($"{selectedDate.ToShortDateString()} {selectedTime}");
            try
            {
                var reservation = _context.Reservation.FirstOrDefault(r => r.ReservationId == reservationId);
                if (reservation == null)
                {
                    TempData["ErrorMessage"] = "Не вдалося знайти актуального бронювання.";
                    return RedirectToAction("ServiceOrder", new {customerId, serviceId});
                }
                var usage = new ServiceUsage
                {
                    ServicesId = serviceId,
                    ReservationId = reservation.ReservationId,
                    ExecutionDate = Date
                };

                _context.ServiceUsage.Add(usage);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.ToString();
                return RedirectToAction("ServiceOrder", new { customerId, serviceId });
            }
            return RedirectToAction("ServiceAccess", new { serviceId = serviceId, reservationId = reservationId, executionDate = Date });
        }
        public IActionResult ServiceAccess(int serviceId, int reservationId, DateTime executionDate)
        {
            var service = _context.Services.FirstOrDefault(s => s.ServicesId == serviceId);
            var reservation = _context.Reservation
                .Include(r => r.Room)
                .Include(c => c.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (service == null || reservation == null)
            {
                TempData["ErrorMessage"] = "Не вдалося знайти інформацію про замовлення.";
                return RedirectToAction("Index");
            }

            var model = new
            {
                ServiceName = service.ServicesName,
                RoomNumber = reservation.Room?.RoomNumber,
                ExecutionDate = executionDate,
                Price = service.Price
            };

            return View(model);
        }
    }
}