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

                foreach (var pm in model.PropertyManagers)
                {
                    PropertyManager manager = new() { Property = property, UserId = pm.UserId, Role = PropertyManagerRoles.Administrator };
                    dbContext.Attach<Property>(manager.Property);
                    dbContext.PropertyManagers!.Add(manager);
                }
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

                    foreach (var im in model.InventoryManagers)
                    {
                        InventoryManager manager = new() { Inventory = inventory, UserId = im.UserId, Role = InventoryManagerRoles.Administrator };
                        context.Attach<Inventory>(manager.Inventory);
                        context.InventoryManagers.Add(manager);
                    }
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
        public List<InventoryItem> GetInventoryItems(int SelectedInventory)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return (from ii in context.InventoryItems
                        join im in context.InventoryManagers!
                            on ii.Inventory.InventoryId equals im.Inventory.InventoryId
                        where im.UserId == _userManager.GetUserId(Context.User)
                            && ii.Inventory.InventoryId == SelectedInventory
                        select ii).ToList();
            }
        }
    }
}
