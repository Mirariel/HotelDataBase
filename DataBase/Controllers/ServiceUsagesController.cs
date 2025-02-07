using DataBase.Extensions;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Controllers
{
    [Authorize]
    public class ServiceUsagesController : Controller
    {
        private readonly HotelDataBaseContext _context;
        private readonly SortingDictionary<ServiceUsage> _serviceUsageSorts = new SortingDictionary<ServiceUsage>
        {
            { "service_desc", q => q.OrderByDescending(s => s.Services.ServicesName) },
            { "employee_desc", q => q.OrderByDescending(s => s.Employee.LastName) },
            { "employee_asc", q => q.OrderBy(s => s.Employee.LastName) },
            { "date_desc", q => q.OrderByDescending(s => s.ExecutionDate) },
            { "date_asc", q => q.OrderBy(s => s.ExecutionDate) }
        };

        public ServiceUsagesController(HotelDataBaseContext context)
        {
            _context = context;
            _serviceUsageSorts.SetDefaultSort(q => q.OrderBy(s => s.ExecutionDate));
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["ServiceSortParam"] = String.IsNullOrEmpty(sortOrder) ? "service_desc" : "";
            ViewData["EmployeeSortParam"] = sortOrder == "employee_desc" ? "employee_asc" : "employee_desc";
            ViewData["ExecutionDateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["CurrentFilter"] = searchString;

            var serviceUsages = _context.ServiceUsage
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Customer)
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Room)
                .Include(s => s.Services)
                .Include(s => s.Employee)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                serviceUsages = serviceUsages.Where(s =>
                    s.Reservation.Customer.FirstName.Contains(searchString) ||
                    s.Reservation.Customer.LastName.Contains(searchString)
                );
            }

            serviceUsages = _serviceUsageSorts.ApplySorting(serviceUsages, sortOrder);

            return View(await serviceUsages.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsage
                .Include(s => s.Reservation)
                .ThenInclude(r => r.Room)
                .Include(s => s.Reservation)
                .ThenInclude(r => r.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.UsageId == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }
        public IActionResult Create()
        {
            SetServiceUsageViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceUsage serviceUsage)
        {
            if (!ModelState.IsValid)
            {
                SetServiceUsageViewData(serviceUsage);
                return View(serviceUsage);
            }
            _context.Add(serviceUsage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsage.FindAsync(id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            SetServiceUsageViewData(serviceUsage);
            return View(serviceUsage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceUsage serviceUsage)
        {
            if (id != serviceUsage.UsageId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                SetServiceUsageViewData(serviceUsage);
                return View(serviceUsage);
            }
            try
            {
                _context.Update(serviceUsage);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceUsageExists(serviceUsage.UsageId))
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

            var serviceUsage = await _context.ServiceUsage
                .Include(s => s.Reservation)
                .ThenInclude(r => r.Customer)
                .Include(s => s.Reservation)
                .ThenInclude(r => r.Room)
                .Include(s => s.Employee)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.UsageId == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceUsage = await _context.ServiceUsage.FindAsync(id);
            if (serviceUsage != null)
            {
                _context.ServiceUsage.Remove(serviceUsage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceUsageExists(int id)
        {
            return _context.ServiceUsage.Any(e => e.UsageId == id);
        }

        private void SetServiceUsageViewData(ServiceUsage? serviceUsage = null)
        {
            ViewData["ReservationId"] = new SelectList(
                _context.Reservation
                    .Include(r => r.Customer)
                    .Include(r => r.Room)
                    .ToList(),
                "ReservationId",
                "DisplayText",
                serviceUsage?.ReservationId
            );
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeesId", "NameWithPosition", serviceUsage?.EmployeeId);
            ViewData["ServicesId"] = new SelectList(_context.Services, "ServicesId", "ServicesName", serviceUsage?.ServicesId);
        }
    }
}