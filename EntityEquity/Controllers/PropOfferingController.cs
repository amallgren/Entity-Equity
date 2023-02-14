﻿using EntityEquity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropOfferingController : ControllerBase
    {
        private ApplicationDbContext _dbContext;
        private UserManager<IdentityUser> _userManager;
        public PropOfferingController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        [HttpGet]
        public List<OfferingWithOrderItem> Get(int propertyId)
        {
            using (var dbContext = _dbContext)
            {
                var offerings = (from o in dbContext.Offerings!.Include(o => o.InventoryItem)
                                 join oi in dbContext.OrderItems!
                                    .Include(oi => oi.Order)
                                    .Include(oi => oi.Property)
                                    on new { OfferingId = o.OfferingId, UserId = _userManager.GetUserId(User), OrderState = OrderState.Incomplete  } 
                                        equals new { OfferingId = oi.Offering.OfferingId, UserId = oi.Order.UserId, OrderState = oi.Order.State }
                                        into oie
                                 from orit in oie.DefaultIfEmpty()
                                 join pom in dbContext.PropertyOfferingMappings!
                                    on o.OfferingId equals pom.Offering!.OfferingId
                                 where pom.Property!.PropertyId == propertyId
                                 select new OfferingWithOrderItem { Offering = o, OrderItem = orit }).ToList();
                return offerings;
            }
        }

    }
}
