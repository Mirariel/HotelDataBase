using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using DataBase.Extensions;

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
        public IActionResult SelectCustomer(BookingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Customer.PassportNumber))
            {
                return this.ViewWithModelError("Номер паспорта обов'язковий для бронювання.", "Error", "Помилка валідації.");
            }

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == request.Customer.PassportNumber);

            if (existingCustomer == null)
            {
                if (string.IsNullOrWhiteSpace(request.Customer.FirstName) ||
                    string.IsNullOrWhiteSpace(request.Customer.LastName) ||
                    string.IsNullOrWhiteSpace(request.Customer.Phone) ||
                    string.IsNullOrWhiteSpace(request.Customer.Email) ||
                    string.IsNullOrWhiteSpace(request.Customer.Address))
                {
                    return this.ViewWithModelError("Для створення нового користувача потрібно заповнити всі поля.", "Error", "Помилка створення користувача.");
                }

                _context.Customers.Add(request.Customer);
                _context.SaveChanges();

                existingCustomer = request.Customer;
            }

            return View(request.Customer);
        }


        [HttpGet]
        public IActionResult SelectDate(string roomType)
        {
            if (string.IsNullOrWhiteSpace(roomType))
            {
                return RedirectToAction("Index");
            }

            SetRoomTypeViewBag(int.Parse(roomType));
            return View();
        }

        [HttpGet]
        public IActionResult SelectRoom(int roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate == default || checkOutDate == default || checkInDate >= checkOutDate)
            {
                SetRoomTypeViewBag(roomType);
                SetDateViewBag(checkInDate, checkOutDate);
                return this.RedirectWithTempError("Будь ласка, введіть правильні дати.", nameof(SelectDate));
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
                SetRoomTypeViewBag(roomType);
                SetDateViewBag(checkInDate, checkOutDate);
                return this.RedirectWithTempError("На жаль, немає доступних кімнат для обраного типу. Спробуйте вибрати інші дати.", nameof(SelectDate));
            }

            SetDateViewBag(checkInDate, checkOutDate);
            return View(availableRooms);
        }
        private void SetDateViewBag(DateTime checkInDate, DateTime checkOutDate)
        {
            ViewBag.CheckInDate = checkInDate == default ? "" : checkInDate.ToString("yyyy-MM-dd");
            ViewBag.CheckOutDate = checkOutDate == default ? "" : checkOutDate.ToString("yyyy-MM-dd");
        }
        private void SetRoomTypeViewBag(int roomType)
        {
            ViewBag.RoomType = roomType;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(ReservationRequest request)
        {
            if (request.RoomId == 0 || string.IsNullOrWhiteSpace(request.Customer.PassportNumber))
            {
                return this.RedirectWithTempError("Некоректні дані.", "SelectRoomType");
            }

            var existingCustomer = _context.Customers.FirstOrDefault(c => c.PassportNumber == request.Customer.PassportNumber);

            if (existingCustomer == null)
            {
                if (string.IsNullOrWhiteSpace(request.Customer.FirstName) || string.IsNullOrWhiteSpace(request.Customer.LastName))
                {
                    return this.RedirectWithTempError("Для створення нового користувача потрібно заповнити всі поля.", "AvailableRooms", new { roomType = ViewBag.RoomType, request.CheckInDate, request.CheckOutDate });
                }

                _context.Customers.Add(request.Customer);
                await _context.SaveChangesAsync();
                existingCustomer = request.Customer;
            }

            var reservation = new Reservation
            {
                CustomerId = existingCustomer.CustomerId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = _context.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == request.RoomId).RoomType.Price * (decimal)(request.CheckOutDate - request.CheckInDate).TotalDays
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

