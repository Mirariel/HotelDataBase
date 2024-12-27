using Microsoft.AspNetCore.Mvc;
using DataBase.Models;

namespace DataBase.Controllers
{
    public class HotelRoomController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public HotelRoomController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public IActionResult Room(int id)
        {
            // Шукаємо кімнату за заданим ID
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == id);

            if (room == null)
            {
                return View("NotFound");
            }

            return View(room);
        }
    }
}

