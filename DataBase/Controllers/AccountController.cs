using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DataBase.Services;
using DataBase.Models;
using DataBase.Extensions;

namespace DataBase.Controllers
{
    public class AccountController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public AccountController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employee = await GetEmployeeByEmailAsync(model.Email);

            if (!IsValidCredentials(employee, model.Password))
                return this.ViewWithModelError("Пошта або пароль неправильні!", model);

            await SignInWithFullNameAsync(employee.FullName);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        private async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
        }

        private async Task SignInWithFullNameAsync(string fullName)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, fullName) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private bool IsValidCredentials(Employee? employee, string password)
        {
            return employee is not null && PasswordHashService.VerifyPassword(password, employee.PasswordHash);
        }
    }
}
