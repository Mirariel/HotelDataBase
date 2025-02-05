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
            var services = GetAllServices();
            return View(services);
        }

        [HttpPost]
        public IActionResult CheckReservation(string passportNumber, int serviceId)
        {
            var customer = FindCustomerByPassport(passportNumber);
            if (customer == null)
            {
                SetTempDataErrorMessage($"Клієнта з номером паспорта {passportNumber} не знайдено.");
                return RedirectToAction("Index");
            }

            return RedirectToAction("ServiceOrder", new { customerId = customer.CustomerId, serviceId });
        }

        public IActionResult ServiceOrder(int customerId, int serviceId)
        {
            var service = FindServiceById(serviceId);
            if (service == null)
            {
                SetTempDataErrorMessage("Послугу не знайдено.");
                return RedirectToAction("Index");
            }

            var reservations = GetActiveReservations(customerId);
            if (!reservations.Any())
            {
                SetTempDataErrorMessage("У клієнта немає активних бронювань.");
                return RedirectToAction("Index");
            }

            SetViewDataForServiceOrder(reservations, customerId, serviceId);
            return View(service);
        }

        [HttpPost]
        public IActionResult ConfirmServiceOrder(ServiceOrderRequest request)
        {
            var executionDate = ParseExecutionDate(request.SelectedDate, request.SelectedTime);

            try
            {
                var reservation = FindReservationById(request.ReservationId);
                if (reservation == null)
                {
                    SetTempDataErrorMessage("Не вдалося знайти актуального бронювання.");
                    return RedirectToAction("ServiceOrder", new { request.CustomerId, request.ServiceId });
                }

                AddServiceUsage(request.ServiceId, reservation.ReservationId, executionDate);
            }
            catch (Exception ex)
            {
                SetTempDataErrorMessage(ex.ToString());
                return RedirectToAction("ServiceOrder", new { request.CustomerId, request.ServiceId });
            }

            return RedirectToAction("ServiceAccess", new { request.ServiceId, request.ReservationId, executionDate });
        }

        public IActionResult ServiceAccess(int serviceId, int reservationId, DateTime executionDate)
        {
            var service = FindServiceById(serviceId);
            var reservation = FindReservationWithDetailsById(reservationId);

            if (service == null || reservation == null)
            {
                SetTempDataErrorMessage("Не вдалося знайти інформацію про замовлення.");
                return RedirectToAction("Index");
            }

            var model = CreateServiceAccessModel(service, reservation, executionDate);
            return View(model);
        }

        private List<Service> GetAllServices()
        {
            return _context.Services.ToList();
        }

        private Customer FindCustomerByPassport(string passportNumber)
        {
            return _context.Customers.FirstOrDefault(c => c.PassportNumber == passportNumber);
        }

        private Service FindServiceById(int serviceId)
        {
            return _context.Services.FirstOrDefault(s => s.ServicesId == serviceId);
        }

        private List<Reservation> GetActiveReservations(int customerId)
        {
            return _context.Reservation
                .Where(r => r.CustomerId == customerId && r.CheckOutDate >= DateTime.Today)
                .Include(r => r.Room)
                .ToList();
        }

        private Reservation FindReservationById(int reservationId)
        {
            return _context.Reservation.FirstOrDefault(r => r.ReservationId == reservationId);
        }

        private Reservation FindReservationWithDetailsById(int reservationId)
        {
            return _context.Reservation
                .Include(r => r.Room)
                .Include(c => c.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);
        }

        private void AddServiceUsage(int serviceId, int reservationId, DateTime executionDate)
        {
            var usage = new ServiceUsage
            {
                ServicesId = serviceId,
                ReservationId = reservationId,
                ExecutionDate = executionDate
            };

            _context.ServiceUsage.Add(usage);
            _context.SaveChanges();
        }

        private DateTime ParseExecutionDate(DateTime selectedDate, string selectedTime)
        {
            return DateTime.Parse($"{selectedDate.ToShortDateString()} {selectedTime}");
        }

        private void SetTempDataErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        private void SetViewDataForServiceOrder(List<Reservation> reservations, int customerId, int serviceId)
        {
            ViewBag.Reservations = reservations;
            ViewBag.CustomerId = customerId;
            ViewBag.ServiceId = serviceId;
        }

        private object CreateServiceAccessModel(Service service, Reservation reservation, DateTime executionDate)
        {
            return new
            {
                ServiceName = service.ServicesName,
                RoomNumber = reservation.Room?.RoomNumber,
                ExecutionDate = executionDate,
                Price = service.Price
            };
        }
    }
}
