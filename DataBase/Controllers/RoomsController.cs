using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataBase.Controllers
{
    public class RoomsController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public RoomsController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["RoomTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "roomtype_desc" : "";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["IsAvailableSortParm"] = sortOrder == "isavailable" ? "isavailable_desc" : "isavailable";

            var rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();

            switch (sortOrder)
            {
                case "roomtype_desc":
                    rooms = rooms.OrderByDescending(r => r.RoomType.TypeName).ToList();
                    break;
                case "capacity":
                    rooms = rooms.OrderBy(r => r.RoomType.Capacity).ToList();
                    break;
                case "capacity_desc":
                    rooms = rooms.OrderByDescending(r => r.RoomType.Capacity).ToList();
                    break;
                case "price":
                    rooms = rooms.OrderBy(r => r.RoomType.Price).ToList();
                    break;
                case "price_desc":
                    rooms = rooms.OrderByDescending(r => r.RoomType.Price).ToList();
                    break;
                case "isavailable":
                    rooms = rooms.OrderBy(r => r.IsAvailable).ToList();
                    break;
                case "isavailable_desc":
                    rooms = rooms.OrderByDescending(r => r.IsAvailable).ToList();
                    break;
                default:
                    rooms = rooms.OrderBy(r => r.RoomType.TypeName).ToList();
                    break;
            }

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
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomNumber,RoomTypeId,IsAvailable")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName", room.RoomTypeId);
            return View(room);
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
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName", room.RoomTypeId);
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

            if (ModelState.IsValid)
            {
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
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "TypeId", "TypeName", room.RoomTypeId);
            return View(room);
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
    }
}
