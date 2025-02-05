using DataBase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DataBase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HotelDataBaseContext _context;

        public HomeController(ILogger<HomeController> logger, HotelDataBaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roomTypes = await _context.RoomTypes.ToListAsync();
            return View(roomTypes);
        }



        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.Rooms
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomType,
                    r.RoomType.Price,
                    r.RoomNumber,
                    r.TypeAndNumber
                })
                .ToListAsync();

            return Json(rooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

