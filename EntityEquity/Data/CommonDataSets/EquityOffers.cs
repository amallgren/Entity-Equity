using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Data.CommonDataSets
{
    public class EquityOffers
    {
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private UserManager<IdentityUser> _userManager;
        private System.Security.Claims.ClaimsPrincipal? _user;
        private string? _slug;
        public EquityOffers(IDbContextFactory<ApplicationDbContext> dbContextFactory, 
            UserManager<IdentityUser> userManager, 
            System.Security.Claims.ClaimsPrincipal? user = null,
            string? slug = null)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _user = user;
            _slug = slug;
        }
        public int GetUserHoldings()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var holdings = (from et1 in dbContext.EquityTransactions
                                   join et2 in dbContext.EquityTransactions
                                        on et1.SellerUserId equals _userManager.GetUserId(_user) into ets1
                                   from etsf1 in ets1.DefaultIfEmpty()
                                   join et3 in dbContext.EquityTransactions
                                       on et1.BuyerUserId equals _userManager.GetUserId(_user) into ets2
                                   from etsf2 in ets2.DefaultIfEmpty()
                                   join p in dbContext.Properties
                                       on et1.Property.PropertyId equals p.PropertyId
                                   where p.Slug == _slug
                                group new { et1, etsf1, etsf2, p } by p.PropertyId into etg
                                   select new UserHoldings()
                                   {
                                       Sells = etg.Sum(t => t.etsf1.Shares),
                                       Buys = etg.Sum(t => t.etsf2.Shares)
                                   }).FirstOrDefault();

                return holdings.Buys - holdings.Sells;
            }
        }
        public List<LiveOffer> GetLiveOffers()
        {
            List<LiveOffer> liveOffers = new List<LiveOffer>();
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var offers = from eo in dbContext.EquityOffers
                             join p in dbContext.Properties
                                on eo.Property.PropertyId equals p.PropertyId
                             where p.Slug == _slug
                             select new { eo, p };

                foreach (var offer in offers)
                {
                    var balance = GetUserHoldings();
                    var liveOffer = new LiveOffer()
                    {
                        LiveOfferId = offer.eo.EquityOfferId,
                        Shares = balance > offer.eo.Shares ? offer.eo.Shares : balance,
                        Price = offer.eo.Price
                    };
                    liveOffers.Add(liveOffer);
                }
            }
            return liveOffers;
        }
        
    }
    public class LiveOffer
    {
        public int LiveOfferId { get; set; }
        public int PropertyUrl { get; set; }
        public int Shares { get; set; }
        public decimal Price { get; set; }
    }
    public class UserHoldings
    {
        public int Buys;
        public int Sells;
    }
}
