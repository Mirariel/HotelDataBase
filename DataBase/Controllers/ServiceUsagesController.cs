using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;

namespace DataBase.Controllers
{
    public class ServiceUsagesController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public ServiceUsagesController(HotelDataBaseContext context)
        {
            _context = context;
        }

        // GET: ServiceUsages
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["ServiceSortParam"] = String.IsNullOrEmpty(sortOrder) ? "service_desc" : "";
            ViewData["EmployeeSortParam"] = sortOrder == "employee_desc" ? "employee_asc" : "employee_desc";
            ViewData["CurrentFilter"] = searchString;

            var serviceUsages = _context.ServiceUsages
                .Include(s => s.Customer)
                .Include(s => s.Services)
                .Include(s => s.Employee)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                serviceUsages = serviceUsages.Where(s =>
                    s.Customer.FirstName.Contains(searchString) ||
                    s.Customer.LastName.Contains(searchString)
                );
            }

            serviceUsages = sortOrder switch
            {
                "service_desc" => serviceUsages.OrderByDescending(s => s.Services.ServicesName),
                "employee_desc" => serviceUsages.OrderByDescending(s => s.Employee.LastName),
                "employee_asc" => serviceUsages.OrderBy(s => s.Employee.LastName),
                _ => serviceUsages.OrderBy(s => s.Services.ServicesName),
            };

            return View(await serviceUsages.ToListAsync());
        }





        // GET: ServiceUsages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.UsageId == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }

        // GET: ServiceUsages/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeesId", "LastName");
            ViewData["ServicesId"] = new SelectList(_context.Services, "ServicesId", "ServicesName");
            return View();
        }

        // POST: ServiceUsages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsageId,CustomerId,ServicesId,EmployeeId")] ServiceUsage serviceUsage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serviceUsage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", serviceUsage.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeesId", "LastName", serviceUsage.EmployeeId);
            ViewData["ServicesId"] = new SelectList(_context.Services, "ServicesId", "ServicesName", serviceUsage.ServicesId);
            return View(serviceUsage);
        }

        // GET: ServiceUsages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages.FindAsync(id);
            if (serviceUsage == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", serviceUsage.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeesId", "LastName", serviceUsage.EmployeeId);
            ViewData["ServicesId"] = new SelectList(_context.Services, "ServicesId", "ServicesName", serviceUsage.ServicesId);
            return View(serviceUsage);
        }

        // POST: ServiceUsages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UsageId,CustomerId,ServicesId,EmployeeId")] ServiceUsage serviceUsage)
        {
            if (id != serviceUsage.UsageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", serviceUsage.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeesId", "LastName", serviceUsage.EmployeeId);
            ViewData["ServicesId"] = new SelectList(_context.Services, "ServicesId", "ServicesName", serviceUsage.ServicesId);
            return View(serviceUsage);
        }

        // GET: ServiceUsages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages
                .Include(s => s.Customer)
                .Include(s => s.Employee)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(m => m.UsageId == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }

        // POST: ServiceUsages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceUsage = await _context.ServiceUsages.FindAsync(id);
            if (serviceUsage != null)
            {
                _context.ServiceUsages.Remove(serviceUsage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceUsageExists(int id)
        {
            return _context.ServiceUsages.Any(e => e.UsageId == id);
        }
    }
}