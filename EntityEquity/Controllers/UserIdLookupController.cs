using EntityEquity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserIdLookupController : Controller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        public UserIdLookupController(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public string? Get(string emailAddress)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var userId = from u in context.Users
                            where u.Email == emailAddress
                            select u.Id;
                if (userId.Count<string>() == 1)
                {
                    return userId.First();
                }
                return null;
            }
        }
    }
}
