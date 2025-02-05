using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DataBase.Controllers
{
    public class HotelRoomController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public HotelRoomController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Room(int type)
        {
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.TypeId == type);

            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
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

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == customer.PassportNumber);

            if (existingCustomer == null)
            {
                if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                    string.IsNullOrWhiteSpace(customer.LastName) ||
                    string.IsNullOrWhiteSpace(customer.Phone) ||
                    string.IsNullOrWhiteSpace(customer.Email) ||
                    string.IsNullOrWhiteSpace(customer.Address))
                {
                    ModelState.AddModelError("", "Для створення нового користувача потрібно заповнити всі поля.");
                    return View("Error", "Помилка створення користувача.");
                }

                _context.Customers.Add(customer);
                _context.SaveChanges();

                existingCustomer = customer;
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult SelectDate(string roomType)
        {
            if (string.IsNullOrWhiteSpace(roomType))
            {
                return RedirectToAction("Index");
            }

            ViewBag.RoomType = roomType;
            return View();
        }

        [HttpGet]
        public IActionResult SelectRoom(int roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate == default || checkOutDate == default || checkInDate >= checkOutDate)
            {
                ViewBag.RoomType = roomType;
                ViewBag.CheckInDate = checkInDate == default ? "" : checkInDate.ToString("yyyy-MM-dd");
                ViewBag.CheckOutDate = checkOutDate == default ? "" : checkOutDate.ToString("yyyy-MM-dd");
                TempData["Error"] = "Будь ласка, введіть правильні дати.";
                return View("SelectDate");
            }

            var availableRooms = _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.RoomType.TypeId == roomType &&
                            !_context.Reservation.Any(res =>
                                res.RoomId == r.RoomId &&
                                ((checkInDate >= res.CheckInDate && checkInDate < res.CheckOutDate) ||
                                 (checkOutDate > res.CheckInDate && checkOutDate <= res.CheckOutDate))))
                .ToList();

            if (!availableRooms.Any())
            {
                ViewBag.RoomType = roomType;
                ViewBag.CheckInDate = checkInDate.ToString("yyyy-MM-dd");
                ViewBag.CheckOutDate = checkOutDate.ToString("yyyy-MM-dd");
                TempData["Error"] = "На жаль, немає доступних кімнат для обраного типу. Спробуйте вибрати інші дати.";
                return View("SelectDate");
            }

            ViewBag.CheckInDate = checkInDate.ToString("yyyy-MM-dd");
            ViewBag.CheckOutDate = checkOutDate.ToString("yyyy-MM-dd");
            return View(availableRooms);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(int roomId, Customer customer, DateTime checkInDate, DateTime checkOutDate)
        {
            if (roomId == 0 || string.IsNullOrWhiteSpace(customer.PassportNumber))
            {
                TempData["Error"] = "Некоректні дані.";
                return RedirectToAction("SelectRoomType");
            }

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == customer.PassportNumber);

            if (existingCustomer == null)
            {
                if (string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
                {
                    TempData["Error"] = "Для створення нового користувача потрібно заповнити всі поля.";
                    return RedirectToAction("AvailableRooms", new { roomType = ViewBag.RoomType, checkInDate, checkOutDate });
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                existingCustomer = customer;
            }

            var reservation = new Reservation
            {
                CustomerId = existingCustomer.CustomerId,
                RoomId = roomId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                TotalPrice = _context.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == roomId).RoomType.Price * (decimal)(checkOutDate - checkInDate).TotalDays
            };

            _context.Reservation.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { reservationId = reservation.ReservationId });
        }

        [HttpGet]
        public IActionResult Confirmation(int reservationId)
        {
            var reservation = _context.Reservation
                .Where(r => r.ReservationId == reservationId)
                .Include(r => r.Room)
                .ThenInclude(rt => rt.RoomType)
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

