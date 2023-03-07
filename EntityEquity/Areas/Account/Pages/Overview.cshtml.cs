using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EntityEquity.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using EntityEquity.Common;

namespace EntityEquity.Areas.Account.Pages
{
    public class OverviewModel : PageModel
    {
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private UserManager<IdentityUser> _userManager;
        private HubConnection? hubConnection;
        private CookieBridgeConnection _cookieBridge;
        [BindProperty]
        public decimal Balance { get; set; }
        public string Cookie;
        public OverviewModel(IDbContextFactory<ApplicationDbContext> dbContextFactory, UserManager<IdentityUser> userManager, CookieBridgeConnection cookieBridge)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _cookieBridge = cookieBridge;
        }
        public void OnGet()
        {
            Cookie = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            hubConnection = _cookieBridge.GetHubConnection("/entityhub", Cookie);
            hubConnection.On("UpdateBalance", () =>
            {
                Balance = GetBalance();
                
            });
            Balance = GetBalance();
        }
        public decimal GetBalance()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                return (from l in dbContext.LedgerEntries
                        where l.UserId == _userManager.GetUserId(User)
                        select l.Amount).Sum();
            }
        }
    }
}
