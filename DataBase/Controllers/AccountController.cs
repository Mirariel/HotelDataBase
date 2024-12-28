using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DataBase.Services;
using DataBase.Models;

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
            {
                return View(model);
            }

            // Знаходимо користувача у вашій базі даних
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == model.Email);

            if (!(employee != null && PasswordHashService.VerifyPassword(model.Password, employee.PasswordHash)))
            {
                ModelState.AddModelError("", "Пошта або пароль неправильні!"); 
                return View(model);
            }
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, employee.FullName),

        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);



            // Створюємо аутентифікацію через кукі
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

    }

}
