﻿using EntityEquity.Data;
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
                }
                else
                {
                    var eItem = (from i in dbContext.OrderItems
                                    join o in dbContext.Orders
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
                        item.Order = existingOrder;
                        if (item.Offering is not null)
                        { 
                            dbContext.Attach(item.Offering);
                            dbContext.OrderItems!.Add(item);
                        }
                    }
                    dbContext.Update(existingOrder);
                }
                dbContext.SaveChanges();
            }
            await Clients.Caller.SendAsync("OnUpdatedOrder");
        } 
    }
}
