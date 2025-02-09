using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;

namespace DataBase.Controllers
{
    [Authorize]
    public class RoomTypesController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public RoomTypesController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roomTypes = await GetRoomTypesAsync();
            return View(roomTypes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var roomType = await GetRoomTypeByIdAsync(id);
            if (roomType == null)
                return NotFound();
            return View(roomType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeId,TypeName,Price,Description,Capacity,ImageUrl")] RoomType roomType)
        {
            if (!ModelState.IsValid)
                return View(roomType);
            await AddRoomTypeAsync(roomType);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var roomType = await GetRoomTypeByIdAsync(id);
            if (roomType == null)
                return NotFound();
            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TypeId,TypeName,Price,Description,Capacity,ImageUrl")] RoomType roomType)
        {
            if (id != roomType.TypeId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(roomType);

            try
            {
                await UpdateRoomTypeAsync(roomType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomTypeExists(roomType.TypeId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var roomType = await GetRoomTypeByIdAsync(id);
            if (roomType == null)
                return NotFound();
            return View(roomType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DeleteRoomTypeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<RoomType>> GetRoomTypesAsync()
        {
            return await _context.RoomTypes.ToListAsync();
        }

        private async Task<RoomType> GetRoomTypeByIdAsync(int? id)
        {
            return id == null ? null : await _context.RoomTypes.FirstOrDefaultAsync(m => m.TypeId == id);
        }

        private async Task AddRoomTypeAsync(RoomType roomType)
        {
            _context.Add(roomType);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateRoomTypeAsync(RoomType roomType)
        {
            _context.Update(roomType);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteRoomTypeAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
                await _context.SaveChangesAsync();
            }
        }
        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.TypeId == id);
        }
    }
}
