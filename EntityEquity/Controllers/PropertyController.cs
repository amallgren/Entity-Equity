using EntityEquity.Data;
using EntityEquity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class PropertyController : Controller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        public PropertyController(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public List<Property> Get(string userId)
        {
            using (var context = _contextFactory.CreateDbContext())
            { 
                if (context.PropertyManagers!=null)
                { 
                    return (from p in context.Properties
                            join pm in context.PropertyManagers
                                on p.PropertyId equals pm.Property.PropertyId
                            where pm.UserId == userId
                            select p).ToList<Property>();
                }
            }
            return new List<Property>();
        }
        [HttpPost]
        public async void Post(PropertyModel model)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (context.Properties != null && context.PropertyManagers != null)
                {
                    Property property = new() { Name = model.Name, Slug = model.Slug };
                    context.Properties.Add(property);
                    await context.SaveChangesAsync();

                    foreach (var pm in model.PropertyManagers)
                    {
                        PropertyManager manager = new() { Property = property, UserId = pm.UserId, Role = PropertyManagerRoles.Administrator };
                        context.Attach<Property>(manager.Property);
                        context.PropertyManagers.Add(manager);
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
