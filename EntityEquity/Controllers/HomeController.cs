using EntityEquity.Data.CommonDataSets;
using EntityEquity.Data;
using EntityEquity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, 
            IDbContextFactory<ApplicationDbContext> dbContextFactory, 
            UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            HomeIndexModel indexModel = new();
            indexModel.Performances = new();

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {

                var performanceIndicators = from p in dbContext.Properties
                                            join oi in dbContext.OrderItems
                                                on p.PropertyId equals oi.Property.PropertyId into oia
                                                from oid in oia.DefaultIfEmpty()
                                            join i in dbContext.Invoices
                                                on oid.Order.OrderId equals i.Order.OrderId into ia
                                                from iad in ia.DefaultIfEmpty()
                                            join ii in dbContext.InvoiceItems
                                                on iad.InvoiceId equals ii.Invoice.InvoiceId into iia
                                                from iiad in iia.DefaultIfEmpty()
                                            where   iad == null
                                                ||  iad.ProcessedAt > DateTime.Now.AddDays(-7)
                                            group new { p, iiad } by new { p.PropertyId } into pr
                                            select new { pr.Key.PropertyId, WeeklySalesAmount = pr.Sum(s => s.iiad.Price) };

                foreach (var performanceIndicator in performanceIndicators)
                {
                    Property property = (from p in dbContext.Properties
                                         where p.PropertyId == performanceIndicator.PropertyId
                                         select p).FirstOrDefault();

                    var equityOffers = new EquityOffers(_dbContextFactory, _userManager, property.Slug);
                    var ownedShares = equityOffers.GetUserHoldings(_userManager.GetUserId(User));

                    PropertyPerformance performance = new()
                    {
                        Property = property,
                        SharesOwned = ownedShares,
                        SalesInLastWeek = performanceIndicator.WeeklySalesAmount
                    };

                    indexModel.Performances.Add(performance);
                }
            }

            return View(indexModel);
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