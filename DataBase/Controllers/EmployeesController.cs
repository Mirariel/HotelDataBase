using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using DataBase.Services;

namespace DataBase.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public EmployeesController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await GetEmployeesAsync();
            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            return employee == null ? NotFound() : View(employee);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
                return View(employee);

            if (string.IsNullOrWhiteSpace(employee.Password))
            {
                ModelState.AddModelError("Password", "Пароль є обов'язковим.");
                return View(employee);
            }

            employee.PasswordHash = PasswordHashService.HashPassword(employee.Password);
            await AddEmployeeAsync(employee);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            return employee == null ? NotFound() : View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeesId)
                return NotFound();

            if (employee.Password != null)
                employee.PasswordHash = PasswordHashService.HashPassword(employee.Password);

            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                await UpdateEmployeeAsync(employee);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeesId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            return employee == null ? NotFound() : View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await RemoveEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        private async Task<Employee?> GetEmployeeByIdAsync(int? id)
        {
            return id == null ? null : await _context.Employees.FirstOrDefaultAsync(m => m.EmployeesId == id);
        }

        private async Task AddEmployeeAsync(Employee employee)
        {
            _context.Add(employee);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
        }

        private async Task RemoveEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeesId == id);
        }
    }
}
