using EntityEquity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Pages
{
    public class PropIndexModel : PageModel
    {
        public string Slug { get; set; }
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public PropIndexModel(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public void OnGet(string slug)
        {
            Slug = slug;
        }
        public List<OfferingWithOrderItem> Offerings
        {
            get
            {
                return GetOfferings();
            }
        }
        private List<OfferingWithOrderItem> GetOfferings()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var offerings = (from o in dbContext.Offerings
                                 join oi in dbContext.OrderItems
                                    on o.OfferingId equals oi.Offering.OfferingId into oie
                                from oi in oie.DefaultIfEmpty()
                                 join pom in dbContext.PropertyOfferingMappings!
                                    on o.OfferingId equals pom.Offering!.OfferingId
                                where pom.Property!.PropertyId == Property.PropertyId
                                select new OfferingWithOrderItem { Offering = o, OrderItem = oi }).ToList();
                return offerings;
            }
        }

        public Property Property
        {
            get
            {
                return GetProperty();
            }
        }
        private Property GetProperty()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var property = (from p in dbContext.Properties
                                where p.Slug == Slug
                                select p).FirstOrDefault();
                return property;
            }
            
        }
    }
}
