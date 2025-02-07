using DataBase.Extensions;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly HotelDataBaseContext _context;

        private readonly SortingDictionary<Reservation> _reservationSorts = new SortingDictionary<Reservation>
        {
            { "checkin_desc", r => r.OrderByDescending(x => x.CheckInDate) },
            { "totalprice", r => r.OrderBy(x => x.TotalPrice) },
            { "totalprice_desc", r => r.OrderByDescending(x => x.TotalPrice) }
        };

        public ReservationsController(HotelDataBaseContext context)
        {
            _context = context;
            _reservationSorts.SetDefaultSort(r => r.OrderBy(x => x.CheckInDate));
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CheckInDateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "checkin_desc" : "";
            ViewData["TotalPriceSortParm"] = sortOrder == "totalprice" ? "totalprice_desc" : "totalprice";
            ViewData["CurrentFilter"] = searchString;

            var reservations = _context.Reservation
                .Include(r => r.Customer)
                .Include(r => r.Room)
                    .ThenInclude(room => room.RoomType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                reservations = reservations.Where(r =>
                    r.Customer.FirstName.Contains(searchString) ||
                    r.Customer.LastName.Contains(searchString));
            }

            reservations = _reservationSorts.ApplySorting(reservations, sortOrder);

            return View(await reservations.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        public IActionResult Create()
        {
            SetCustomerRoomViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,CustomerId,RoomId,CheckInDate,CheckOutDate")] Reservation reservation)
        {
            try
            {
                if (reservation.CheckInDate >= reservation.CheckOutDate)
                {
                    ModelState.AddModelError("", "Check-out date must be after check-in date.");
                }

                var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomId == reservation.RoomId);

                if (room == null)
                {
                    ModelState.AddModelError("", "Selected room does not exist.");
                }

                if (ModelState.IsValid)
                {
                    SetReservationTotalPrice(reservation, room.RoomType);
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToString());
            }
            SetCustomerRoomViewData(reservation);
            return View(reservation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            SetCustomerRoomViewData(reservation);
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,CustomerId,RoomId,CheckInDate,CheckOutDate")] Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");
            }

            var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomId == reservation.RoomId);

            if (room == null)
            {
                ModelState.AddModelError("", "Selected room does not exist.");
            }

            if (!ModelState.IsValid)
            {
                SetCustomerRoomViewData(reservation);
                return View(reservation);
            }
            try
            {
                SetReservationTotalPrice(reservation, room.RoomType);

                _context.Update(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(reservation.ReservationId))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservation
                .Include(r => r.Room)
                .ThenInclude(r => r.RoomType)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sqlQuery = "DELETE FROM Reservation WHERE ReservationID = @id";
            await _context.Database.ExecuteSqlRawAsync(sqlQuery, new SqlParameter("@id", id));
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservation.Any(e => e.ReservationId == id);
        }

        private void SetCustomerRoomViewData(Reservation? reservation = null)
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", reservation?.CustomerId);
            ViewData["RoomId"] = new SelectList(_context.Rooms.Include(room => room.RoomType), "RoomId", "TypeAndNumber", reservation?.RoomId);
        }
        private void SetReservationTotalPrice(Reservation reservation, RoomType? roomType)
        {
            int numberOfDays = (reservation.CheckOutDate - reservation.CheckInDate).Days;
            if (roomType != null)
                reservation.TotalPrice = numberOfDays * roomType.Price;
        }
    }
}