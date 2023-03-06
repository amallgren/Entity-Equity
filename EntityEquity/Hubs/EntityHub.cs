using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using EntityEquity.Common;
using EntityEquity.Data;
using EntityEquity.Data.CommonDataSets;
using EntityEquity.Data.Models;
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
        private IConfiguration _configuration;
        public EntityHub(IDbContextFactory<ApplicationDbContext> dbContextFactor, 
            UserManager<IdentityUser> userManager, 
            IConfiguration configuration)
        {
            _dbContextFactory = dbContextFactor;
            _userManager = userManager;
            _configuration = configuration;
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
                    ShowPublicInsights = model.PublicInsights,
                    OwnerUserId = _userManager.GetUserId(Context.User)};
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
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                return (from p in dbContext.Properties
                        join pm in dbContext.PropertyManagers!
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
                            offering.MustShip = model.MustShip;

                            context.Offerings!.Add(offering);
                            await context.SaveChangesAsync();
                            if (model.PhotoUrl is not null)
                            {
                                PhotoUrl photoUrl = new() { Url = model.PhotoUrl.Url };
                                context.PhotoUrls.Add(photoUrl);
                                await context.SaveChangesAsync();

                                OfferingPhotoUrlMapping photoMapping = new OfferingPhotoUrlMapping();
                                photoMapping.PhotoUrl = photoUrl;
                                photoMapping.Offering = offering;
                                context.OfferingPhotoUrlMappings.Add(photoMapping);
                                await context.SaveChangesAsync();
                            }

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
        public async Task<int> GetIncompleteOrderNumber()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var orderId = await (from o in dbContext.Orders
                                   where o.UserId == _userManager.GetUserId(Context.User)
                                        && o.State == OrderState.Incomplete
                                   select o.OrderId).FirstOrDefaultAsync();
                return orderId;
            }
        }
        [Authorize]
        public async Task<List<lineItemType>> GetLineItems()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var listItems = await (from oi in dbContext.OrderItems.Include(i => i.Offering)
                                      join o in dbContext.Orders
                                           on oi.Order.OrderId equals o.OrderId
                                      where o.UserId == _userManager.GetUserId(Context.User)
                                           && o.State == OrderState.Incomplete
                                      select oi).ToListAsync();

                List<lineItemType> returnList = new();

                foreach (var listItem in listItems)
                {
                    lineItemType newItem = new() {
                        itemId = listItem.Offering.OfferingId.ToString(),
                        name = listItem.Offering.Name,
                        quantity = listItem.Quantity,
                        unitPrice = listItem.Offering.Price
                    };
                    returnList.Add(newItem);
                }

                return returnList;
            }
        }
        [Authorize]
        public async Task<PaymentMethod> GetPaymentMethod()
        {
            try { 
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var order = await(from o in dbContext.Orders
                                      where o.UserId == _userManager.GetUserId(Context.User)
                                              && o.State == OrderState.Incomplete
                                      select o).FirstOrDefaultAsync();
                    if (order is not null)
                    { 
                        bool eCheck = (from of in dbContext.Offerings
                                       join oi in dbContext.OrderItems
                                           on of.OfferingId equals oi.Offering.OfferingId
                                       where of.MustPayWithECheck == true
                                           && oi.Order.OrderId == order.OrderId
                                       select of).Any();
                        if (eCheck)
                            return PaymentMethod.eCheck;
                    }
                }
            }
            catch(Exception ex)
            {
                string breakpoint = "";
            }
            return PaymentMethod.CreditCard;
        }
        public async Task<FinalizeOrderReturnModel> FinalizeCreditCardOrder(CreditCardPaymentParameters parameters)
        {
            FinalizeOrderReturnModel returnObject = null;
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    var order = await (from o in dbContext.Orders
                                       where o.UserId == _userManager.GetUserId(Context.User)
                                               && o.State == OrderState.Incomplete
                                       select o).FirstOrDefaultAsync();
                    Payment payment = new Payment(_configuration, _dbContextFactory, _userManager, Context.User);
                    PaymentResult result = new();

                    result = await payment.RunCard(parameters);
                    var billingAddress = await SaveBillingAddress(parameters);
                    order.BillingAddress = billingAddress;
                    await dbContext.SaveChangesAsync();

                    var register = await GetPaymentRegister(order);
                    register = await ProcessPlatformFee(order, register, PaymentMethod.CreditCard);

                    await DistributeCOGSPayments(order, register);
                    await DistributeProfitShares(order, register);
                    await GenerateInvoices();

                    order.State = OrderState.Complete;
                    await dbContext.SaveChangesAsync();

                    var mustShip = (from oi in dbContext.OrderItems.Include(i => i.Offering)
                                    where oi.Order.OrderId == order.OrderId
                                        && oi.Offering.MustShip
                                    select oi).Any();

                    returnObject = new()
                    {
                        Order = order,
                        Result = result,
                        MustShip = mustShip
                    };
                }
            }
            catch (Exception ex)
            {
                string breakpoint = "";
            }
            await Clients.Caller.SendAsync("OnFinalizedOrder");
            return returnObject;
        }
        public async Task<FinalizeOrderReturnModel> FinalizedECheckOrder(eCheckPaymentParameters parameters)
        {
            FinalizeOrderReturnModel returnObject = null;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var order = await (from o in dbContext.Orders
                                   where o.UserId == _userManager.GetUserId(Context.User)
                                           && o.State == OrderState.Incomplete
                                   select o).FirstOrDefaultAsync();
                Payment payment = new Payment(_configuration, _dbContextFactory, _userManager, Context.User);
                PaymentResult result = new();

                result = await payment.RunCheck((eCheckPaymentParameters)parameters);

                if (result.Successful)
                {
                    var register = await GetPaymentRegister(order);  
                    ProcessPlatformFee(order, register, PaymentMethod.eCheck);
                    await DistributeCOGSPayments(order, register);
                    await DistributeProfitShares(order, register);
                    await GenerateInvoices();
                    order.State = OrderState.Complete;
                    await dbContext.SaveChangesAsync();

                    var mustShip = (from oi in dbContext.OrderItems.Include(i => i.Offering)
                                    where oi.Order.OrderId == order.OrderId
                                        && oi.Offering.MustShip
                                    select oi).Any();

                    returnObject = new()
                    {
                        Order = order,
                        Result = result,
                        MustShip = mustShip
                    };
                }
            }
            await Clients.Caller.SendAsync("OnFinalizedOrder");
            return returnObject;
        }
        private async Task<PaymentRegister> GetPaymentRegister(Order order)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            { 
                var books  = await (from p in dbContext.Properties
                               join pom in dbContext.PropertyOfferingMappings
                                   on p.PropertyId equals pom.Property.PropertyId
                               join of in dbContext.Offerings
                                   on pom.Offering.OfferingId equals of.OfferingId
                               join oi in dbContext.OrderItems
                                   on of.OfferingId equals oi.Offering.OfferingId
                               join ii in dbContext.InventoryItems
                                   on oi.Offering.InventoryItem.InventoryItemId equals ii.InventoryItemId
                               where oi.Order.OrderId == order.OrderId
                               group new { p, of } by new { p.PropertyId } into pof
                               select new PropertyBook
                               {
                                   PropertyId = pof.Key.PropertyId,
                                   Amount = pof.Sum(ofa => ofa.of.Price),
                                   Deductions = 0
                               }).ToListAsync();
                PaymentRegister register = new PaymentRegister();
                register.Books = books;
                return register;
            }
        }
        private async Task<PaymentRegister> ProcessPlatformFee(Order order, PaymentRegister register, PaymentMethod paymentMethod)
        {
            decimal baseAmount = 0;
            decimal percentage = 0;
            string userAccountId = String.Empty;
            if (paymentMethod == PaymentMethod.CreditCard)
            {
                IConfigurationSection section = _configuration.GetSection("PlatformFee:CreditCard");
                baseAmount = section.GetValue<decimal>("Base");
                percentage = section.GetValue<decimal>("Percentage")/100;
            }
            else if (paymentMethod == PaymentMethod.eCheck)
            {
                IConfigurationSection section = _configuration.GetSection("PlatformFee:eCheck");
                baseAmount = section.GetValue<decimal>("Base");
                percentage = section.GetValue<decimal>("Percentage") / 100;
            }
            IConfigurationSection receivableSection = _configuration.GetSection("PlatformFee:AccountReceivable");
            userAccountId = receivableSection.GetValue<string>("UserId");
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var platformFees = await (from p in dbContext.Properties
                                   join u in dbContext.Users
                                     on p.OwnerUserId equals u.Id
                                   join pom in dbContext.PropertyOfferingMappings
                                       on p.PropertyId equals pom.Property.PropertyId
                                   join of in dbContext.Offerings
                                       on pom.Offering.OfferingId equals of.OfferingId
                                   join oi in dbContext.OrderItems
                                       on of.OfferingId equals oi.Offering.OfferingId
                                   where oi.Order.OrderId == order.OrderId
                                   group new { p, u, of } by new { p.PropertyId, u.Id } into puf
                                   select new PlatformFee { PlatformId = puf.Key.PropertyId, 
                                       UserId = puf.Key.Id, 
                                       Amount = (puf.Sum(ofa => ofa.of.Price) * percentage) + baseAmount }).ToListAsync();

                foreach (PlatformFee platformFee in platformFees)
                {
                    decimal amount = Math.Round(platformFee.Amount, 2);
                    register.Deduct(platformFee.PlatformId, amount);

                    string description = paymentMethod == PaymentMethod.CreditCard ?
                        $"Platform Fee - Credit Card - Order #{order.OrderId} - Property #{platformFee.PlatformId} - {platformFee.UserId}"
                        : $"Platform Fee - eCheck - Order #{order.OrderId} - Property #{platformFee.PlatformId} - {platformFee.UserId}";

                    LedgerEntry entry = new()
                    {
                        UserId = userAccountId,
                        Amount = amount,
                        Description = description
                    };

                    dbContext.LedgerEntries.Add(entry);
                }
                await dbContext.SaveChangesAsync();
            }
            return register;
        }
        private async Task<PaymentRegister> DistributeCOGSPayments(Order order, PaymentRegister register)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var cogss = await (from p in dbContext.Properties
                                  join pom in dbContext.PropertyOfferingMappings
                                      on p.PropertyId equals pom.Property.PropertyId
                                  join of in dbContext.Offerings
                                      on pom.Offering.OfferingId equals of.OfferingId
                                  join oi in dbContext.OrderItems
                                      on of.OfferingId equals oi.Offering.OfferingId
                                  join ii in dbContext.InventoryItems
                                      on oi.Offering.InventoryItem.InventoryItemId equals ii.InventoryItemId
                                   where oi.Order.OrderId == order.OrderId
                                  group new { p, oi, ii } by new { p.PropertyId, PropertyName = p.Name, p.OwnerUserId } into ps
                                  select new { ps.Key.PropertyId, ps.Key.PropertyName, ps.Key.OwnerUserId, Amount = ps.Sum(c => c.ii.Cost * c.oi.Quantity) }).ToListAsync();

                foreach (var cogs in cogss)
                {
                    decimal amount;
                    decimal remaining = register.GetRemaining(cogs.PropertyId);
                    if (remaining < cogs.Amount)
                    {
                        register.Deduct(cogs.PropertyId, remaining);
                        amount = remaining;
                    }
                    else
                    {
                        register.Deduct(cogs.PropertyId, cogs.Amount);
                        amount = cogs.Amount;
                    }

                    LedgerEntry entry = new()
                    {
                        UserId = cogs.OwnerUserId,
                        Description = $"COGS Payment - Property #{cogs.PropertyId} - Property Name: {cogs.PropertyName} - Order #{order.OrderId}",
                        Amount = amount
                    };
                    dbContext.LedgerEntries.Add(entry);
                }
                await dbContext.SaveChangesAsync();
            }
            return register;
        }
        private async Task DistributeProfitShares(Order order, PaymentRegister register)
        {
            List<Shareholder> shareholders = await GetShareholders(order);
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                foreach (var shareholder in shareholders)
                {
                    decimal payout = register.GetRemaining(shareholder.PropertyId) * ((decimal)shareholder.Shares/shareholder.PropertyShares);
                    if (payout > 0)
                    { 
                        payout = Math.Round(payout, 2);
                        LedgerEntry entry = new()
                        {
                            UserId = shareholder.UserId,
                            Description = $"Profit Payout - Property #{shareholder.PropertyId} - Property Name: {shareholder.PropertyName} - Order #{order.OrderId}",
                            Amount = payout
                        };
                        dbContext.LedgerEntries.Add(entry);
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
        private async Task<List<Shareholder>> GetShareholders(Order order)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var shares = await (from et in dbContext.EquityTransactions
                            where (from pom in dbContext.PropertyOfferingMappings
                                           join of in dbContext.Offerings
                                               on pom.Offering.OfferingId equals of.OfferingId
                                           join oi in dbContext.OrderItems
                                               on of.OfferingId equals oi.Offering.OfferingId
                                           where oi.Order.OrderId == order.OrderId
                                           select pom.Property.PropertyId).Contains(et.Property.PropertyId)
                            group new { et } by new { et.Property.PropertyId, PropertyName = et.Property.Name, PropertyShares = et.Property.Shares, et.BuyerUserId } into bet
                            where (bet.Sum(e => e.et.Shares) - 
                                dbContext.EquityTransactions.Where(ets => ets.Property.PropertyId == bet.Key.PropertyId 
                                    && ets.SellerUserId == bet.Key.BuyerUserId)
                                .Select(ets => ets.Shares)
                                .Sum()) > 0

                            //(from ets in dbContext.EquityTransactions
                            //                                   where ets.Property.PropertyId == bet.Key.PropertyId
                            //                                      && ets.SellerUserId == bet.Key.BuyerUserId
                            //                                   select ets.Shares).Sum()) > 0
                                    select new Shareholder
                            {
                                PropertyId = bet.Key.PropertyId,
                                PropertyName = bet.Key.PropertyName,
                                PropertyShares = bet.Key.PropertyShares,
                                UserId = bet.Key.BuyerUserId,
                                Shares = bet.Sum(e => e.et.Shares) - (from etsi in dbContext.EquityTransactions
                                                                      where etsi.Property.PropertyId == bet.Key.PropertyId
                                                                         && etsi.SellerUserId == bet.Key.BuyerUserId
                                                                      select etsi.Shares).Sum()
                            }).ToListAsync();
                return shares;
            }
        }
        private async Task GenerateInvoices()
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
        private async Task<BillingAddress> SaveBillingAddress(CreditCardPaymentParameters parameters)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var newAddress = new BillingAddress()
                {
                    UserId = _userManager.GetUserId(Context.User),
                    FirstName = parameters.BillingAddress.firstName,
                    LastName = parameters.BillingAddress.lastName,
                    StreetAddress = parameters.BillingAddress.address,
                    City = parameters.BillingAddress.city,
                    ZipCode = parameters.BillingAddress.zip
                };
                dbContext.BillingAddresses.Add(newAddress);
                await dbContext.SaveChangesAsync();
                return newAddress;
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
        [Authorize]
        public async Task SaveShippingAddress(Order order, ShippingAddress newAddress)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                newAddress.UserId = _userManager.GetUserId(Context.User);
                dbContext.ShippingAddresses.Add(newAddress);
                await dbContext.SaveChangesAsync();

                order.ShippingAddress = newAddress;
                dbContext.Orders.Attach(order);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
