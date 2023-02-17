using EntityEquity.Data;
using EntityEquity.Data.CommonDataSets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Areas.Prep.Pages
{
    public class EquityModel : PageModel
    {
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private UserManager<IdentityUser> _userManager;
        public string Slug;
        public int Balance;
        public EquityModel(IDbContextFactory<ApplicationDbContext> dbContextFactory, UserManager<IdentityUser> userManager)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
        }
        public void OnGet(string slug)
        {
            Slug = slug;
            Balance = GetBalance();
        }
        private int GetBalance()
        {
            EquityOffers dataset = new(_dbContextFactory, _userManager, User, Slug);
            var holdings = dataset.GetUserHoldings();
            return holdings;
        }
    }
}
