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
        private readonly SortingDictionary<Reservation> _reservationSorts;

        public ReservationsController(HotelDataBaseContext context)
        {
            _context = context;
            _reservationSorts = InitializeSortingDictionary();
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            SetIndexViewData(sortOrder, searchString);
            var reservations = GetFilteredReservations(searchString);
            reservations = _reservationSorts.ApplySorting(reservations, sortOrder);
            return View(await reservations.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var reservation = await GetReservationWithDetails(id);
            return reservation == null ? NotFound() : View(reservation);
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
                var room = await GetRoomWithDetails(reservation.RoomId);
                ValidateReservation(reservation, room);

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
            var reservation = await GetReservationById(id);
            if (reservation == null) 
                return NotFound();
            SetCustomerRoomViewData(reservation);
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,CustomerId,RoomId,CheckInDate,CheckOutDate")] Reservation reservation)
        {
            if (id != reservation.ReservationId) 
                return NotFound();
            var room = await GetRoomWithDetails(reservation.RoomId);
            ValidateReservation(reservation, room);

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
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var reservation = await GetReservationWithDetails(id);
            return reservation == null ? NotFound() : View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DeleteReservationById(id);
            return RedirectToAction(nameof(Index));
        }

        private SortingDictionary<Reservation> InitializeSortingDictionary()
        {
            var sortingDictionary = new SortingDictionary<Reservation>
            {
                { "checkin_desc", r => r.OrderByDescending(x => x.CheckInDate) },
                { "totalprice", r => r.OrderBy(x => x.TotalPrice) },
                { "totalprice_desc", r => r.OrderByDescending(x => x.TotalPrice) }
            };
            sortingDictionary.SetDefaultSort(r => r.OrderBy(x => x.CheckInDate));
            return sortingDictionary;
        }

        private void SetIndexViewData(string sortOrder, string searchString)
        {
            ViewData["CheckInDateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "checkin_desc" : "";
            ViewData["TotalPriceSortParm"] = sortOrder == "totalprice" ? "totalprice_desc" : "totalprice";
            ViewData["CurrentFilter"] = searchString;
        }

        private IQueryable<Reservation> GetFilteredReservations(string searchString)
        {
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
            return reservations;
        }

        private async Task<Reservation> GetReservationWithDetails(int? id)
        {
            return id == null ? null : await _context.Reservation
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
        }

        private async Task<Reservation> GetReservationById(int? id)
        {
            return id == null ? null : await _context.Reservation.FindAsync(id);
        }

        private async Task<Room> GetRoomWithDetails(int roomId)
        {
            return await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomId == roomId);
        }

        private void ValidateReservation(Reservation reservation, Room room)
        {
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");
            }
            if (room == null)
            {
                ModelState.AddModelError("", "Selected room does not exist.");
            }
        }

        private async Task DeleteReservationById(int id)
        {
            var sqlQuery = "DELETE FROM Reservation WHERE ReservationID = @id";
            await _context.Database.ExecuteSqlRawAsync(sqlQuery, new SqlParameter("@id", id));
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
