using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;

namespace DataBase.Controllers
{
    public class RoomsController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public RoomsController(HotelDataBaseContext context)
        {
            _context = context;
        }

        private async Task UpdateRoomAvailability()
        {
            var today = DateTime.Today;

            var reservations = await _context.Reservations.ToListAsync();

            var rooms = await _context.Rooms.ToListAsync();

            foreach (var room in rooms)
            {
                var isBooked = reservations.Any(r =>
                    r.RoomId == room.RoomId &&
                    r.CheckInDate <= today &&
                    r.CheckOutDate >= today);

                room.IsAvailable = !isBooked;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["RoomTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "roomtype_desc" : "";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["IsAvailableSortParm"] = sortOrder == "isavailable" ? "isavailable_desc" : "isavailable";

            var rooms = await GetRoomsWithAvailability();

            switch (sortOrder)
            {
                case "roomtype_desc":
                    rooms = rooms.OrderByDescending(r => r.RoomType).ToList();
                    break;
                case "capacity":
                    rooms = rooms.OrderBy(r => r.Capacity).ToList();
                    break;
                case "capacity_desc":
                    rooms = rooms.OrderByDescending(r => r.Capacity).ToList();
                    break;
                case "price":
                    rooms = rooms.OrderBy(r => r.Price).ToList();
                    break;
                case "price_desc":
                    rooms = rooms.OrderByDescending(r => r.Price).ToList();
                    break;
                case "isavailable":
                    rooms = rooms.OrderBy(r => r.IsAvailable).ToList();
                    break;
                case "isavailable_desc":
                    rooms = rooms.OrderByDescending(r => r.IsAvailable).ToList();
                    break;
                default:
                    rooms = rooms.OrderBy(r => r.RoomType).ToList();
                    break;
            }

            return View(rooms);
        }


        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomNumber,RoomType,Capacity,Price,Description,IsAvailable")] Room room)
        {
            if (ModelState.IsValid)
            {
                // Додаємо кімнату до бази даних
                _context.Add(room);
                await _context.SaveChangesAsync();

                // Оновлюємо статус доступності після додавання
                await UpdateRoomAvailability();

                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,RoomType,Capacity,Price,Description,IsAvailable")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Оновлюємо кімнату
                    _context.Update(room);
                    await _context.SaveChangesAsync();

                    // Оновлюємо статус доступності після редагування
                    await UpdateRoomAvailability();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                // Оновлюємо статус доступності після видалення
                await UpdateRoomAvailability();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Room>> GetRoomsWithAvailability()
        {
            var today = DateTime.Today;

            var rooms = await _context.Rooms.ToListAsync();
            var reservations = await _context.Reservations.ToListAsync();

            foreach (var room in rooms)
            {
                room.IsAvailable = !reservations.Any(r =>
                    r.RoomId == room.RoomId &&
                    r.CheckInDate <= today &&
                    r.CheckOutDate >= today);
            }

            return rooms;
        }
        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}