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
    public class ServicesController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ServicesController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await GetServicesAsync();
            return View(services);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var service = await GetServiceByIdAsync(id);
            if (service == null) 
                return NotFound();
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
                return View(service);
            await AddServiceAsync(service);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var service = await GetServiceByIdAsync(id);
            if (service == null) 
                return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ServicesId) 
                return NotFound();
            if (!ModelState.IsValid)
                return View(service);

            try
            {
                await UpdateServiceAsync(service);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.ServicesId)) 
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var service = await GetServiceByIdAsync(id);
            if (service == null) 
                return NotFound();
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DeleteServiceAsync(id);
            return RedirectToAction(nameof(Index));
        }
        private async Task<List<Service>> GetServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

        private async Task<Service> GetServiceByIdAsync(int? id)
        {
            return id == null ? null : await _context.Services.FirstOrDefaultAsync(m => m.ServicesId == id);
        }

        private async Task AddServiceAsync(Service service)
        {
            _context.Add(service);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateServiceAsync(Service service)
        {
            _context.Update(service);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServicesId == id);
        }
    }
}
