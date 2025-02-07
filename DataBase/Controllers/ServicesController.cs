using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ServicesController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServicesId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);

            }
            _context.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ServicesId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(service);
            }
            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.ServicesId))
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

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServicesId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServicesId == id);
        }
    }
}
