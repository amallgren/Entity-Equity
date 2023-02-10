using Microsoft.AspNetCore.Mvc;

namespace EntityEquity.Controllers
{
    public class ProController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
