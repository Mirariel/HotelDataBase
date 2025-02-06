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
            {
                return View(model);
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == model.Email);

            if (!(employee != null && PasswordHashService.VerifyPassword(model.Password, employee.PasswordHash)))
            {
                return this.ViewWithModelError("Пошта або пароль неправильні!", model);
            }
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, employee.FullName),

        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);



            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }

}
