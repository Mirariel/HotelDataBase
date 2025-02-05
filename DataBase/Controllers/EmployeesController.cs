using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using DataBase.Services;
using Microsoft.AspNetCore.Authorization;

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
            return View(await _context.Employees.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeesId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(employee.Password))
                {
                    ModelState.AddModelError("Password", "Пароль є обов'язковим.");
                    return View(employee);
                }

                employee.PasswordHash = PasswordHashService.HashPassword(employee.Password);

                _context.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmployeesId)
            {
                return NotFound();
            }
             if (employee.Password!=null)
            {
                employee.PasswordHash=PasswordHashService.HashPassword(employee.Password);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeesId))
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
            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeesId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeesId == id);
        }
    }
}
