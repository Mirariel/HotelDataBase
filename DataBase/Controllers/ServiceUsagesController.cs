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
        private readonly SortingDictionary<ServiceUsage> _serviceUsageSorts;

        public ServiceUsagesController(HotelDataBaseContext context)
        {
            _context = context;
            _serviceUsageSorts = InitializeSortingDictionary();
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            SetIndexViewData(sortOrder, searchString);

            var serviceUsages = GetFilteredAndSortedServiceUsages(searchString, sortOrder);
            return View(await serviceUsages.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var serviceUsage = await GetServiceUsageByIdAsync(id);
            if (serviceUsage == null) 
                return NotFound();
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
            await AddServiceUsageAsync(serviceUsage);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var serviceUsage = await GetServiceUsageByIdAsync(id);
            if (serviceUsage == null) 
                return NotFound();
            SetServiceUsageViewData(serviceUsage);
            return View(serviceUsage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceUsage serviceUsage)
        {
            if (id != serviceUsage.UsageId) 
                return NotFound();
            if (!ModelState.IsValid)
            {
                SetServiceUsageViewData(serviceUsage);
                return View(serviceUsage);
            }
            try
            {
                await UpdateServiceUsageAsync(serviceUsage);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceUsageExists(serviceUsage.UsageId)) 
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var serviceUsage = await GetServiceUsageByIdAsync(id);
            if (serviceUsage == null) 
                return NotFound();
            return View(serviceUsage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DeleteServiceUsageAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private SortingDictionary<ServiceUsage> InitializeSortingDictionary()
        {
            var sortingDictionary = new SortingDictionary<ServiceUsage>
            {
                { "service_desc", q => q.OrderByDescending(s => s.Services.ServicesName) },
                { "employee_desc", q => q.OrderByDescending(s => s.Employee.LastName) },
                { "employee_asc", q => q.OrderBy(s => s.Employee.LastName) },
                { "date_desc", q => q.OrderByDescending(s => s.ExecutionDate) },
                { "date_asc", q => q.OrderBy(s => s.ExecutionDate) }
            };
            sortingDictionary.SetDefaultSort(q => q.OrderBy(s => s.ExecutionDate));
            return sortingDictionary;
        }

        private void SetIndexViewData(string sortOrder, string searchString)
        {
            ViewData["ServiceSortParam"] = string.IsNullOrEmpty(sortOrder) ? "service_desc" : "";
            ViewData["EmployeeSortParam"] = sortOrder == "employee_desc" ? "employee_asc" : "employee_desc";
            ViewData["ExecutionDateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["CurrentFilter"] = searchString;
        }

        private IQueryable<ServiceUsage> GetFilteredAndSortedServiceUsages(string searchString, string sortOrder)
        {
            var query = _context.ServiceUsage
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Customer)
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Room)
                .Include(s => s.Services)
                .Include(s => s.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.Reservation.Customer.FirstName.Contains(searchString) ||
                    s.Reservation.Customer.LastName.Contains(searchString)
                );
            }

            return _serviceUsageSorts.ApplySorting(query, sortOrder);
        }

        private async Task<ServiceUsage> GetServiceUsageByIdAsync(int? id)
        {
            return id == null ? null : await _context.ServiceUsage
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Customer)
                .Include(s => s.Reservation)
                    .ThenInclude(r => r.Room)
                .Include(s => s.Employee)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.UsageId == id);
        }

        private async Task AddServiceUsageAsync(ServiceUsage serviceUsage)
        {
            _context.Add(serviceUsage);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateServiceUsageAsync(ServiceUsage serviceUsage)
        {
            _context.Update(serviceUsage);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteServiceUsageAsync(int id)
        {
            var serviceUsage = await _context.ServiceUsage.FindAsync(id);
            if (serviceUsage != null)
            {
                _context.ServiceUsage.Remove(serviceUsage);
                await _context.SaveChangesAsync();
            }
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
