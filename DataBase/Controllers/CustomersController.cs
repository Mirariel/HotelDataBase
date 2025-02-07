using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;

namespace DataBase.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public CustomersController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await GetCustomersAsync();
            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var customer = await GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,FirstName,LastName,Birthday,Phone,Email,PassportNumber,Address")] Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            await AddCustomerAsync(customer);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var customer = await GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FirstName,LastName,Birthday,Phone,Email,PassportNumber,Address")] Customer customer)
        {
            if (id != customer.CustomerId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                await UpdateCustomerAsync(customer);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.CustomerId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var customer = await GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await RemoveCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Customer>> GetCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        private async Task<Customer?> GetCustomerByIdAsync(int? id)
        {
            return id == null ? null : await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == id);
        }

        private async Task AddCustomerAsync(Customer customer)
        {
            _context.Add(customer);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Update(customer);
            await _context.SaveChangesAsync();
        }

        private async Task RemoveCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}