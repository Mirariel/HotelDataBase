using Microsoft.AspNetCore.Mvc;
using DataBase.Models;
using Microsoft.EntityFrameworkCore;
using DataBase.Extensions;
using Azure.Core;

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
            var roomType = await GetRoomTypeByIdAsync(type);
            return roomType == null ? NotFound() : View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectCustomer(BookingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Customer.PassportNumber))
                return this.ViewWithModelError("Номер паспорта обов'язковий для бронювання.", "Error", "Помилка валідації.");

            var existingCustomer = await GetCustomerByPassportAsync(request.Customer.PassportNumber);

            if (existingCustomer != null)
                return View(request.Customer);

            if (IsCustomerNotComplete(request.Customer))
                return this.ViewWithModelError("Для створення нового користувача потрібно заповнити всі поля.", "Error", "Помилка створення користувача.");

            await AddCustomerAsync(request.Customer);
            return View(request.Customer);
        }

        [HttpGet]
        public IActionResult SelectDate(string roomType)
        {
            if (string.IsNullOrWhiteSpace(roomType))
                return RedirectToAction("Index");

            SetRoomTypeViewBag(int.Parse(roomType));
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SelectRoom(int roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            if (!IsBookingPeriodValid(checkInDate, checkOutDate))
            {
                SetRoomTypeViewBag(roomType);
                SetDateViewBag(checkInDate, checkOutDate);
                return this.ViewWithTempError("Будь ласка, введіть правильні дати.", nameof(SelectDate));
            }

            var availableRooms = await GetAvailableRoomsAsync(roomType, checkInDate, checkOutDate);

            if (!availableRooms.Any())
            {
                SetRoomTypeViewBag(roomType);
                SetDateViewBag(checkInDate, checkOutDate);
                return this.ViewWithTempError("На жаль, немає доступних кімнат для обраного типу. Спробуйте вибрати інші дати.", nameof(SelectDate));
            }

            SetDateViewBag(checkInDate, checkOutDate);
            return View(availableRooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(ReservationRequest request)
        {
            if (request.RoomId == 0 || string.IsNullOrWhiteSpace(request.Customer.PassportNumber))
                return this.ViewWithTempError("Некоректні дані.", "SelectRoomType");

            var customer = await EnsureCustomerExistsAsync(request.Customer);
            if (customer is null)
                return this.ViewWithTempError("Для створення нового користувача потрібно заповнити всі поля.", "AvailableRooms", new { roomType = ViewBag.RoomType, request.CheckInDate, request.CheckOutDate });

            var totalDays = GetTotalDays(request.CheckInDate, request.CheckOutDate);
            var totalPrice = await CalculateTotalPriceAsync(request.RoomId, totalDays);

            var reservation = new Reservation
            {
                CustomerId = customer.CustomerId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = totalPrice
            };

            await AddReservationAsync(reservation);
            return RedirectToAction("Confirmation", new { reservationId = reservation.ReservationId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int reservationId)
        {
            var reservation = await GetReservationDetailsAsync(reservationId);
            return reservation == null ? View("Error", "Бронювання не знайдено.") : View(reservation);
        }

        private async Task<RoomType> GetRoomTypeByIdAsync(int typeId)
        {
            return await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.TypeId == typeId);
        }

        private async Task<Customer> GetCustomerByPassportAsync(string passportNumber)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.PassportNumber == passportNumber);
        }

        private async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }
        private int GetTotalDays(DateTime checkInDate, DateTime checkOutDate)
        {
            return (checkOutDate - checkInDate).Days;
        }
        private bool IsBookingPeriodValid(DateTime checkInDate, DateTime checkOutDate)
        {
            return checkInDate != default && checkOutDate != default && checkInDate <= checkOutDate;
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

        private async Task<decimal> CalculateTotalPriceAsync(int roomId, double totalDays)
        {
            var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomId == roomId);
            return room?.RoomType.Price * (decimal)totalDays ?? 0;
        }

        private async Task AddReservationAsync(Reservation reservation)
        {
            _context.Reservation.Add(reservation);
            await _context.SaveChangesAsync();
        }

        private bool IsCustomerNotComplete(Customer customer)
        {
            if (customer is null)
                return true;

            return string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName) ||
                string.IsNullOrWhiteSpace(customer.Phone) ||
                string.IsNullOrWhiteSpace(customer.Email) ||
                string.IsNullOrWhiteSpace(customer.Address);
        }

        private async Task<List<Room>> GetAvailableRoomsAsync(int roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.RoomType.TypeId == roomType &&
                            !_context.Reservation.Any(res =>
                                res.RoomId == r.RoomId &&
                                ((checkInDate >= res.CheckInDate && checkInDate < res.CheckOutDate) ||
                                 (checkOutDate > res.CheckInDate && checkOutDate <= res.CheckOutDate))))
                .ToListAsync();
        }

        private async Task<Customer?> EnsureCustomerExistsAsync(Customer customer)
        {
            var existingCustomer = await GetCustomerByPassportAsync(customer.PassportNumber);
            if (existingCustomer != null)
                return existingCustomer;

            if (string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
                return null;

            await AddCustomerAsync(customer);
            return customer;
        }

        private async Task<object> GetReservationDetailsAsync(int reservationId)
        {
            return await _context.Reservation
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
                .FirstOrDefaultAsync();
        }
    }
}

