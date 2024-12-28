using Microsoft.AspNetCore.Mvc;
using DataBase.Models;

namespace DataBase.Controllers
{
    public class HotelRoomController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public HotelRoomController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Room(string type)
        {

            if (type == null)
            {
                return View("NotFound");
            }

            return View((object) type);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectCustomer(Customer customer, int roomId, string roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            if (string.IsNullOrWhiteSpace(customer.PassportNumber))
            {
                ModelState.AddModelError("", "Номер паспорта обов'язковий для бронювання.");
                return View("Error", "Помилка валідації.");
            }

            // Пошук існуючого користувача за номером паспорта
            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == customer.PassportNumber);

            if (existingCustomer == null)
            {
                // Якщо користувача немає, перевіряємо, чи всі обов'язкові поля заповнені
                if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                    string.IsNullOrWhiteSpace(customer.LastName) ||
                    string.IsNullOrWhiteSpace(customer.Phone) ||
                    string.IsNullOrWhiteSpace(customer.Email) ||
                    string.IsNullOrWhiteSpace(customer.Address))
                {
                    ModelState.AddModelError("", "Для створення нового користувача потрібно заповнити всі поля.");
                    return View("Error", "Помилка створення користувача.");
                }

                // Додаємо нового користувача до бази даних
                _context.Customers.Add(customer);
                _context.SaveChanges();

                // Отримуємо ідентифікатор нового користувача
                existingCustomer = customer;
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult SelectDate(string roomType)
        {
            if (string.IsNullOrWhiteSpace(roomType))
            {
                return RedirectToAction("SelectRoom");
            }

            ViewBag.RoomType = roomType;
            return View();
        }

        [HttpGet]
        public IActionResult SelectRoomType()
        {
            var roomTypes = _context.Rooms
                .Select(r => r.RoomType)
                .Distinct()
                .ToList();

            return View(roomTypes);
        }

        [HttpGet]
        public IActionResult SelectRoom(string roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            if (string.IsNullOrWhiteSpace(roomType) || checkInDate == default || checkOutDate == default)
            {
                return RedirectToAction("Index", "Home");
            }

            // Шукаємо доступні кімнати
            var availableRooms = _context.Rooms
          /*      .Where(r => r.RoomType == roomType &&
                            !_context.Reservations.Any(res =>
                                res.RoomId == r.RoomId &&
                                ((checkInDate >= res.CheckInDate && checkInDate < res.CheckOutDate) ||
                                 (checkOutDate > res.CheckInDate && checkOutDate <= res.CheckOutDate)))) */
                .ToList();

            if (!availableRooms.Any())
            {
                TempData["Error"] = "На жаль, немає доступних кімнат для обраного типу.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.RoomType = roomType;
            ViewBag.CheckInDate = checkInDate;
            ViewBag.CheckOutDate = checkOutDate;

            return View(availableRooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateReservation(int roomId, Customer customer, DateTime checkInDate, DateTime checkOutDate)
        {
            if (roomId == 0 || string.IsNullOrWhiteSpace(customer.PassportNumber))
            {
                TempData["Error"] = "Некоректні дані.";
                return RedirectToAction("SelectRoomType");
            }

            // Спроба знайти користувача
            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == customer.PassportNumber);

            if (existingCustomer == null)
            {
                // Перевіряємо заповненість обов'язкових полів
                if (string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
                {
                    TempData["Error"] = "Для створення нового користувача потрібно заповнити всі поля.";
                    return RedirectToAction("AvailableRooms", new { roomType = ViewBag.RoomType, checkInDate, checkOutDate });
                }

                _context.Customers.Add(customer);
                _context.SaveChanges();
                existingCustomer = customer;
            }

            // Створення бронювання
            var reservation = new Reservation
            {
                CustomerId = existingCustomer.CustomerId,
                RoomId = roomId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                TotalPrice = _context.Rooms.First(r => r.RoomId == roomId).RoomType.Price * (decimal)(checkOutDate - checkInDate).TotalDays
            };

            _context.Reservation.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("Confirmation", new { reservationId = reservation.ReservationId });
        }

        [HttpGet]
        public IActionResult Confirmation(int reservationId)
        {
            var reservation = _context.Reservation
                .Where(r => r.ReservationId == reservationId)
                .Select(r => new
                {
                    r.ReservationId,
                    r.CheckInDate,
                    r.CheckOutDate,
                    r.TotalPrice,
                    CustomerName = r.Customer.FirstName + " " + r.Customer.LastName,
                    RoomNumber = r.Room.RoomNumber,
                    RoomType = r.Room.RoomType
                })
                .FirstOrDefault();

            if (reservation == null)
            {
                return View("Error", "Бронювання не знайдено.");
            }

            return View(reservation);
        }


    }
}

