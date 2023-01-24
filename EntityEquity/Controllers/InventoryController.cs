using EntityEquity.Data;
using EntityEquity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EntityEquity.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class InventoryController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        public InventoryController(ILogger<HomeController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, IConfiguration configuration)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            var baseAddress = _configuration["BaseAddress"];
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InventoryIndexModel model = new(baseAddress, userId);
            using (var context = _contextFactory.CreateDbContext())
            {
                model.Inventories = GetInventories(context, userId);
            }
            return View(model);
        }
        private List<Inventory> GetInventories(ApplicationDbContext context, string userId)
        {
            return (from i in context.Inventories
                    join im in context.InventoryManager!
                        on i.InventoryId equals im.Inventory.InventoryId
                    where im.UserId == userId
                    select i).ToList();
        }
        [HttpPost]
        public async void Post(InventoryModel model)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.Inventories != null && context.InventoryManager != null)
                {
                    Inventory inventory = new() { Name = model.Name };
                    context.Inventories.Add(inventory);
                    await context.SaveChangesAsync();

                    foreach (var im in model.InventoryManagers)
                    {
                        InventoryManager manager = new() { Inventory = inventory, UserId = im.UserId, Role = InventoryManagerRoles.Administrator };
                        context.Attach<Inventory>(manager.Inventory);
                        context.InventoryManager.Add(manager);
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
