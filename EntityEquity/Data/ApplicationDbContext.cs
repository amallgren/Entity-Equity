using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityEquity.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Property>? Properties { get; set; }
        public DbSet<PropertyManager>? PropertyManagers { get; set; }
        public DbSet<Offering>? Offerings { get; set; }
        public DbSet<PropertyOfferingMapping>? PropertyOfferingMappings { get; set; }
        public DbSet<Inventory>? Inventories { get; set; }
        public DbSet<InventoryManager>? InventoryManager { get; set; }
    }
    public class Property
    {
        public Property()
        {
            Name = "";
            Slug = "";
        }
        public int PropertyId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }
    public class PropertyManager
    {
        public PropertyManager()
        {
            Property = new();
        }
        public int PropertyManagerId { get; set; }
        public Property Property { get; set; }
        public string? UserId { get; set; }
        public PropertyManagerRoles Role { get; set; }
    }
    public enum PropertyManagerRoles { Administrator }
    public class Offering
    {
        public Offering()
        {
            this.Name = "";
            this.Description = "";
            InventoryItem = new();
        }
        public int OfferingId { get; set; }
        public InventoryItem InventoryItem { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName="decimal(18,2)")]
        public decimal Price { get; set; }
    }
    public class PhotoUrl
    {
        public PhotoUrl()
        {
            Url = "";
            Offering = new();
        }
        public int PhotoUrlId { get; set; }
        public string Url { get; set; }
        public Offering Offering { get; set; }
    }
    public class OfferingPhotoUrlMapping
    {
        public OfferingPhotoUrlMapping()
        {
            Offering = new();
            PhotoUrl = new();
        }
        public int OfferingPhotoUrlMappingId { get; set; }
        public Offering Offering { get; set; }
        public PhotoUrl PhotoUrl { get; set; }
    }
    public class PropertyOfferingMapping
    {
        public int PropertyOfferingMappingId { get; set; }
        public Property? Property { get; set; }
        public Offering? Offering { get; set; }
    }
    public class EquityShare
    {
        public EquityShare()
        {
            Property = new();
            UserId = "";
        }
        public int EquityShareId { get; set; }
        public Property Property { get; set; }
        public string UserId { get; set; }
        public decimal Percentage { get; set; }
    }
    public class Inventory
    {
        public Inventory()
        {
            Name = "";
        }
        public int InventoryId { get; set; }
        public string Name { get; set; }
    }
    public class InventoryItem
    {
        public InventoryItem()
        {
            Inventory = new();
            Name = "";
            SKU = "";
        }
        public int InventoryItemId { get; set; }
        public Inventory Inventory { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
    }
    public class InventoryManager
    {
        public InventoryManager()
        {
            UserId = "";
            Inventory = new();
        }
        public int InventoryManagerId { get; set; }
        public string UserId { get; set; }
        public Inventory Inventory { get; set; }
        public InventoryManagerRoles Role { get; set; }
    }
    public enum InventoryManagerRoles { Administrator };
}