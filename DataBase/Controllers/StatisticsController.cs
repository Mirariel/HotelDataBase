using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly HotelDataBaseContext _context;

        public StatisticsController(HotelDataBaseContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IncomeByPeriod(DateTime startDate, DateTime endDate)
        {
            var income = GetIncomeByPeriodAsync(startDate, endDate);
            return View("IncomeByPeriod", income);
        }

        public ActionResult TopServices()
        {
            var topServices = GetTopServicesAsync();
            return View("TopServices", topServices);
        }

        public ActionResult TopEmployees()
        {
            var topEmployees = GetTopEmployeesAsync();
            return View("TopEmployees", topEmployees);
        }

        private IncomeResult GetIncomeByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return _context.Database.SqlQueryRaw<IncomeResult>(
                "EXEC GetIncomeByPeriod @StartDate = {0}, @EndDate = {1}",
                startDate, endDate).AsEnumerable().FirstOrDefault();
        }

        private List<TopService> GetTopServicesAsync()
        {
            return _context.Database.SqlQueryRaw<TopService>("EXEC GetTopServices").ToList();
        }

        private List<TopEmployee> GetTopEmployeesAsync()
        {
            return _context.Database.SqlQueryRaw<TopEmployee>("EXEC GetTopEmployeesByServiceUsage").ToList();
        }
    }

    public class IncomeResult
    {
        public decimal ReservationIncome { get; set; }
        public decimal ServiceIncome { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public class TopService
    {
        public string ServicesName { get; set; }
        public int UsageCount { get; set; }
    }

    public class TopEmployee
    {
        public string EmployeeName { get; set; }
        public int ServiceCount { get; set; }
    }
}
