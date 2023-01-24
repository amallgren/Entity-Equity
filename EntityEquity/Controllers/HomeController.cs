using EntityEquity.Data;
using EntityEquity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EntityEquity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, IConfiguration configuration)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var baseAddress = _configuration["BaseAddress"];
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            IndexModel model = new(baseAddress, userId);
            using (var context = _contextFactory.CreateDbContext())
            {
                model.Properties = GetProperties(context, userId);
            }
            return View(model);
        }

        private List<Property>? GetProperties(ApplicationDbContext context, string UserId)
        {
            return (from p in context.Properties
             join pm in context.PropertyManagers!
                on p.PropertyId equals pm.Property.PropertyId
             where pm.Role == PropertyManagerRoles.Administrator
                && pm.UserId == UserId
             select p).ToList();
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