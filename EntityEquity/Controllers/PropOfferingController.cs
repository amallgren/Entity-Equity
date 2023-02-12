using EntityEquity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EntityEquity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropOfferingController : ControllerBase
    {
        private ApplicationDbContext _dbContext;
        public PropOfferingController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public List<OfferingWithOrderItem> Get(int propertyId)
        {
            using (var dbContext = _dbContext)
            {
                var offerings = (from o in dbContext.Offerings
                                 join oi in dbContext.OrderItems
                                    on o.OfferingId equals oi.Offering.OfferingId into oie
                                 from oi in oie.DefaultIfEmpty()
                                 join pom in dbContext.PropertyOfferingMappings!
                                    on o.OfferingId equals pom.Offering!.OfferingId
                                 where pom.Property!.PropertyId == propertyId
                                 select new OfferingWithOrderItem { Offering = o, OrderItem = oi }).ToList();
                return offerings;
            }
        }

    }
}
