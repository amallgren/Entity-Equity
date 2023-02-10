using EntityEquity.Data;
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
                Property property = new() { Name = model.Name, Slug = model.Slug };
                dbContext.Properties!.Add(property);
                await dbContext.SaveChangesAsync();

                PropertyManager propertyManager = new() { Property = property, UserId = _userManager.GetUserId(Context.User), Role = PropertyManagerRoles.Administrator };
                dbContext.Attach<Property>(propertyManager.Property);
                dbContext.PropertyManagers!.Add(propertyManager);

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
        public List<OfferingWithProperty> GetOfferings(int[] propertyIds, int[] inventoryIds)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var offerings = (from o in context.Offerings!.Include("InventoryItem")
                        join om in context.OfferingManagers!
                            on o.OfferingId equals om.Offering.OfferingId
                        join pom in context.PropertyOfferingMappings!
                            on o.OfferingId equals pom.Offering!.OfferingId
                        join p in context.Properties!
                            on pom.Property!.PropertyId equals p.PropertyId
                        join pm in context.PropertyManagers!
                            on pom.Property!.PropertyId equals pm.Property.PropertyId
                        join ii in context.InventoryItems!
                            on o.InventoryItem.InventoryItemId equals ii.InventoryItemId
                        join im in context.InventoryManagers!
                            on o.InventoryItem.Inventory.InventoryId equals im.Inventory.InventoryId
                        where om.UserId == _userManager.GetUserId(Context.User)
                            && om.Role == OfferingManagerRoles.Administrator
                            && pm.UserId == _userManager.GetUserId(Context.User)
                            && pm.Role == PropertyManagerRoles.Administrator
                            && im.UserId == _userManager.GetUserId(Context.User)
                            && im.Role == InventoryManagerRoles.Administrator
                            && propertyIds.Contains(pom.Property!.PropertyId)
                            && inventoryIds.Contains(o.InventoryItem.Inventory.InventoryId)
                        select new OfferingWithProperty() { Offering = o, Property = p }).ToList();

                return offerings;
            }
        }
    }
    [Serializable]
    public class OfferingWithProperty
    {
        public Offering? Offering;
        public Property? Property;
    }
}
