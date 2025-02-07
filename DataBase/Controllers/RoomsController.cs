using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using DataBase.Extensions;

namespace DataBase.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly HotelDataBaseContext _context;
        private readonly SortingDictionary<Room> _roomSorts = new SortingDictionary<Room>
        {
            { "roomtype_desc", r => r.OrderByDescending(x => x.RoomType.TypeName) },
            { "capacity", r => r.OrderBy(x => x.RoomType.Capacity) },
            { "capacity_desc", r => r.OrderByDescending(x => x.RoomType.Capacity) },
            { "price", r => r.OrderBy(x => x.RoomType.Price) },
            { "price_desc", r => r.OrderByDescending(x => x.RoomType.Price) },
            { "isavailable", r => r.OrderBy(x => x.IsAvailable) },
            { "isavailable_desc", r => r.OrderByDescending(x => x.IsAvailable) }
        };
        public RoomsController(HotelDataBaseContext context)
        {
            _context = context;
            _roomSorts.SetDefaultSort(r => r.OrderBy(x => x.RoomType.TypeName));
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["RoomTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "roomtype_desc" : "";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["IsAvailableSortParm"] = sortOrder == "isavailable" ? "isavailable_desc" : "isavailable";

            var rooms = _context.Rooms.Include(r => r.RoomType).AsQueryable();

            rooms = _roomSorts.ApplySorting(rooms, sortOrder);

            return View(rooms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }


        public IActionResult Create()
        {
            SetRoomTypeViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomNumber,RoomTypeId,IsAvailable")] Room room)
        {
            if (!ModelState.IsValid)
            {
                SetRoomTypeViewData(room.RoomTypeId);
                return View(room);
            }
            _context.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

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
            SetRoomTypeViewData(room.RoomTypeId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,RoomTypeId,IsAvailable")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                SetRoomTypeViewData(room.RoomTypeId);
                return View(room);
            }
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.RoomId))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
        private void SetRoomTypeViewData(int? roomTypeId = null)
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName", roomTypeId);
        }
    }
}
