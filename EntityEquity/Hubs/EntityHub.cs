using EntityEquity.Data;
using EntityEquity.Data.CommonDataSets;
using EntityEquity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EntityEquity.Hubs
{
    public class EntityHub : Hub
    {
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private UserManager<IdentityUser> _userManager;
        public EntityHub(IDbContextFactory<ApplicationDbContext> dbContextFactor, UserManager<IdentityUser> userManager)
        {
            _dbContextFactory = dbContextFactor;
            _userManager = userManager;
        }
        [Authorize]
        public async Task AddProperty(PropertyModel model)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                Property property = new() { 
                    Name = model.Name, 
                    Slug = model.Slug, 
                    Shares = model.Shares,
                    AllowEquityOffers = model.EquityOffers, 
                    ShowPublicInsights = model.PublicInsights };
                dbContext.Properties!.Add(property);
                await dbContext.SaveChangesAsync();

                PropertyManager propertyManager = new() { Property = property, UserId = _userManager.GetUserId(Context.User), Role = PropertyManagerRoles.Administrator };
                dbContext.Attach<Property>(propertyManager.Property);
                dbContext.PropertyManagers!.Add(propertyManager);

                EquityTransaction equityTransaction = new()
                {
                    BuyerUserId = _userManager.GetUserId(Context.User),
                    Price = 0,
                    EquityOffer = null,
                    Property = property,
                    Shares = model.Shares,
                    SellerUserId = "Initial creation of shares.",
                };

                dbContext.EquityTransactions.Add(equityTransaction);
                await dbContext.SaveChangesAsync();
            }
            await Clients.Caller.SendAsync("OnAddedProperty");
        }
        [Authorize]
        public List<Property> GetProperties()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return (from p in context.Properties
                        join pm in context.PropertyManagers!
                           on p.PropertyId equals pm.Property.PropertyId
                        where pm.Role == PropertyManagerRoles.Administrator
                           && pm.UserId == _userManager.GetUserId(Context.User)
                        select p).ToList();
            }
        }
        public Property GetProperty(int propertyId)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                return (from p in dbContext.Properties
                                where p.PropertyId == propertyId
                                select p).FirstOrDefault();
            }
        }
        [Authorize]
        public async Task AddAnInventory(InventoryModel model)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                if (context.Inventories != null && context.InventoryManagers != null)
                {
                    Inventory inventory = new() { Name = model.Name };
                    context.Inventories.Add(inventory);
                    await context.SaveChangesAsync();

                    InventoryManager inventoryManager = new() { Inventory = inventory, UserId = _userManager.GetUserId(Context.User), Role = InventoryManagerRoles.Administrator };
                    context.Attach<Inventory>(inventoryManager.Inventory);
                    context.InventoryManagers.Add(inventoryManager);
                    await context.SaveChangesAsync();
                }
            }
            await Clients.Caller.SendAsync("OnAddedInventory");
        }
        [Authorize]
        public List<Inventory> GetInventories()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return (from i in context.Inventories
                        join im in context.InventoryManagers!
                            on i.InventoryId equals im.Inventory.InventoryId
                        where im.UserId == _userManager.GetUserId(Context.User)
                            && im.Role == InventoryManagerRoles.Administrator
                        select i).ToList();
            }
        }
        [Authorize]
        public async Task AddInventoryItem(InventoryItem item)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                if (context.Inventories is not null  
                    && context.InventoryManagers is not null
                    && context.InventoryItems is not null)
                {
                    var inventory = (from i in context.Inventories
                                    join im in context.InventoryManagers
                                        on i.InventoryId equals im.Inventory.InventoryId
                                    where i.InventoryId == item.Inventory.InventoryId
                                        && im.UserId == _userManager.GetUserId(Context.User)
                                    select i).FirstOrDefault();
                    if (inventory is not null)
                    {
                        item.Inventory = inventory;
                        context.InventoryItems.Add(item);
                        await context.SaveChangesAsync();
                    }
                    
                }
            }
            await Clients.Caller.SendAsync("OnAddedInventoryItem");
        }
        [Authorize]
        public List<InventoryItem> GetInventoryItems(int[] selectedInventories)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return (from ii in context.InventoryItems
                        join im in context.InventoryManagers!
                            on ii.Inventory.InventoryId equals im.Inventory.InventoryId
                        where im.UserId == _userManager.GetUserId(Context.User)
                            && im.Role == InventoryManagerRoles.Administrator
                            && selectedInventories.Contains(ii.Inventory.InventoryId) 
                        select ii).ToList();
            }
        }
        [Authorize]
        public async Task AddOfferings(OfferingModel model)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                foreach (var propertyId in model.PropertyIds)
                {
                    foreach (var inventoryItemId in model.InventoryItemIds)
                    {
                        InventoryItem? item = (from ii in context.InventoryItems
                                              join i in context.Inventories!
                                                 on ii.Inventory.InventoryId equals i.InventoryId
                                              join im in context.InventoryManagers!
                                                  on i.InventoryId equals im.Inventory.InventoryId
                                              where im.Role == InventoryManagerRoles.Administrator
                                                  && im.UserId == _userManager.GetUserId(Context.User)
                                                  && ii.InventoryItemId == inventoryItemId
                                              select ii).FirstOrDefault();

                        Property? property = (from p in context.Properties
                                             join pm in context.PropertyManagers!
                                                on p.PropertyId equals pm.Property.PropertyId
                                             where pm.Role == PropertyManagerRoles.Administrator
                                                && pm.UserId == _userManager.GetUserId(Context.User)
                                                && p.PropertyId == propertyId
                                             select p).FirstOrDefault();

                        if (item is not null && property is not null)
                        {
                            Offering offering = new Offering();
                            offering.Slug = model.Slug;
                            offering.Name = model.Name;
                            offering.Description = model.Description;
                            offering.InventoryItem = item;
                            offering.Price = model.Price;

                            context.Offerings!.Add(offering);
                            context.SaveChanges();

                            PropertyOfferingMapping mapping = new PropertyOfferingMapping();
                            mapping.Offering = offering;
                            mapping.Property = property;

                            context.PropertyOfferingMappings!.Add(mapping);

                            OfferingManager offeringManager = new() { Offering = offering, UserId = _userManager.GetUserId(Context.User), Role = OfferingManagerRoles.Administrator };
                            context.Attach<Offering>(offering);
                            context.OfferingManagers!.Add(offeringManager);

                            context.SaveChanges();
                        }
                    }
                }
            }
            await Clients.Caller.SendAsync("OnAddedOffering");
        }
        [Authorize]
        public async Task UpdateOrder(OrderItem item)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                Order? existingOrder = (from o in dbContext.Orders
                            where o.UserId == _userManager.GetUserId(Context.User)
                                && o.State == OrderState.Incomplete
                            select o).FirstOrDefault();
                if (existingOrder is null)
                {
                    existingOrder = new() { UserId = _userManager.GetUserId(Context.User), State = OrderState.Incomplete };
                    dbContext.Orders!.Add(existingOrder);
                    await dbContext.SaveChangesAsync();
                    await CreateOrderItem(existingOrder, item);
                }
                else
                {
                    await CreateOrderItem(existingOrder, item);
                }
            }
            await Clients.Caller.SendAsync("OnUpdatedOrder");
        }
        [Authorize]
        private async Task CreateOrderItem(Order order, OrderItem item)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var eItem = (from i in dbContext.OrderItems
                             join o in dbContext.Orders!
                             on i.Order.OrderId equals o.OrderId
                             where o.UserId == _userManager.GetUserId(Context.User)
                             && o.State == OrderState.Incomplete
                             && i.Offering!.OfferingId == item.Offering!.OfferingId
                             select i).FirstOrDefault();
                if (eItem is not null)
                {
                    eItem.Quantity = item.Quantity;
                }
                else
                {
                    item.Order = order;
                    if (item.Offering is not null && item.Property is not null)
                    {
                        dbContext.Attach(item.Offering);
                        dbContext.Attach(item.Property);
                        dbContext.Attach(item.Order);
                        dbContext.OrderItems!.Add(item);
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
        [Authorize]
        public async Task FinalizeOrder()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var ordersAndProperties = (from o in dbContext.Orders
                                  join oi in dbContext.OrderItems!
                                   on o.OrderId equals oi.Order!.OrderId
                                  where o.UserId == _userManager.GetUserId(Context.User)
                                   && o.State == OrderState.Incomplete
                                  select new { Order = o, Property = oi.Property }).Distinct().ToList();
                foreach (var op in ordersAndProperties)
                {
                    Invoice invoice = new() { Order = op.Order, Property = op.Property, UserId = _userManager.GetUserId(Context.User), ProcessedAt = DateTime.Now };
                    var items = from oi in dbContext.OrderItems.Include(oi => oi.Offering).Include(oi => oi.Offering.InventoryItem)
                                where oi.Order.OrderId == op.Order.OrderId
                                    && oi.Property.PropertyId == op.Property.PropertyId
                                select oi;
                    dbContext.Invoices!.Add(invoice);
                    await dbContext.SaveChangesAsync();
                    foreach (var item in items)
                    {
                        InvoiceItem invoiceItem = new InvoiceItem() { Invoice = invoice, Name = item.Offering.Name, Cost = item.Offering.InventoryItem.Cost, Price = item.Offering.Price, Quantity = item.Quantity };
                        dbContext.InvoiceItems!.Add(invoiceItem);
                    }
                    op.Order.State = OrderState.Complete;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        [Authorize]
        public async Task AddEquityOffer(PrepEquityModel model)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                Data.CommonDataSets.EquityOffers dataset = new(_dbContextFactory, _userManager, model.PropertySlug);
                int balance = dataset.GetUserHoldings(_userManager.GetUserId(Context.User));

                if (balance < model.Shares)
                {
                    throw new Exception("Balance is less than number of shares.");
                }

                var property = (from p in dbContext.Properties
                               where p.Slug == model.PropertySlug
                               select p).FirstOrDefault();

                EquityOffer offer = new() { 
                    UserId = _userManager.GetUserId(Context.User), 
                    Property = property, 
                    Shares = model.Shares, 
                    Price = model.Price,
                    MustPurchaseAll = model.MustPurchaseAll };

                dbContext.EquityOffers.Add(offer);

                await dbContext.SaveChangesAsync();
            }
        }
        [Authorize]
        public List<LiveOffer> GetLiveEquityOffers(string slug)
        {
            EquityOffers dataset = new EquityOffers(_dbContextFactory, _userManager, slug);
            return dataset.GetLiveOffers();
        }
        [Authorize]
        public async Task BuyAllEquityForOrder(int equityOfferId)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                EquityOffer offer = (from eo in dbContext.EquityOffers.Include(e => e.Property)
                                    where eo.EquityOfferId == equityOfferId
                                    select eo).FirstOrDefault();

                int purchasedShares = (from et in dbContext.EquityTransactions
                                       where et.EquityOffer.EquityOfferId == equityOfferId
                                       group et by et.EquityOffer.EquityOfferId into etg
                                       select etg.Sum(e => e.Shares)).FirstOrDefault();

                int shares = offer.Shares - purchasedShares;

                EquityTransaction newTransaction = new EquityTransaction()
                {
                    EquityOffer = offer,
                    BuyerUserId = _userManager.GetUserId(Context.User),
                    Property = offer.Property,
                    Shares = shares,
                    SellerUserId = offer.UserId,
                    Price = offer.Price
                };

                dbContext.EquityTransactions.Add(newTransaction);
                await dbContext.SaveChangesAsync();
            }
            
        }
    }
}
