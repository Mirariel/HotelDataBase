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
        private readonly SortingDictionary<Room> _roomSorts;

        public RoomsController(HotelDataBaseContext context)
        {
            _context = context;
            _roomSorts = InitializeSortingDictionary();
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            SetSortingViewData(sortOrder);
            var rooms = _context.Rooms.Include(r => r.RoomType).AsQueryable();
            rooms = _roomSorts.ApplySorting(rooms, sortOrder);
            return View(rooms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var room = await GetRoomByIdAsync(id);
            if (room == null) 
                return NotFound();
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
            await AddRoomAsync(room);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var room = await GetRoomByIdAsync(id);
            if (room == null) 
                return NotFound();
            SetRoomTypeViewData(room.RoomTypeId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,RoomTypeId,IsAvailable")] Room room)
        {
            if (id != room.RoomId) 
                return NotFound();
            if (!ModelState.IsValid)
            {
                SetRoomTypeViewData(room.RoomTypeId);
                return View(room);
            }
            if (!await UpdateRoomAsync(room)) 
                return NotFound();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var room = await GetRoomByIdAsync(id);
            if (room == null) 
                return NotFound();
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DeleteRoomAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private SortingDictionary<Room> InitializeSortingDictionary()
        {
            var sortingDictionary = new SortingDictionary<Room>
            {
                { "roomtype_desc", r => r.OrderByDescending(x => x.RoomType.TypeName) },
                { "capacity", r => r.OrderBy(x => x.RoomType.Capacity) },
                { "capacity_desc", r => r.OrderByDescending(x => x.RoomType.Capacity) },
                { "price", r => r.OrderBy(x => x.RoomType.Price) },
                { "price_desc", r => r.OrderByDescending(x => x.RoomType.Price) },
                { "isavailable", r => r.OrderBy(x => x.IsAvailable) },
                { "isavailable_desc", r => r.OrderByDescending(x => x.IsAvailable) }
            };
            sortingDictionary.SetDefaultSort(r => r.OrderBy(x => x.RoomType.TypeName));
            return sortingDictionary;
        }

        private void SetSortingViewData(string sortOrder)
        {
            ViewData["RoomTypeSortParm"] = string.IsNullOrEmpty(sortOrder) ? "roomtype_desc" : "";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["IsAvailableSortParm"] = sortOrder == "isavailable" ? "isavailable_desc" : "isavailable";
        }

        private async Task<Room> GetRoomByIdAsync(int? id)
        {
            return id == null ? null : await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(m => m.RoomId == id);
        }

        private async Task AddRoomAsync(Room room)
        {
            _context.Add(room);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> UpdateRoomAsync(Room room)
        {
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return RoomExists(room.RoomId);
            }
        }

        private async Task DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
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
