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
        public DbSet<OfferingManager>? OfferingManagers { get; set; }
        public DbSet<PropertyOfferingMapping>? PropertyOfferingMappings { get; set; }
        public DbSet<Inventory>? Inventories { get; set; }
        public DbSet<InventoryManager>? InventoryManagers { get; set; }
        public DbSet<InventoryItem>? InventoryItems { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderItem>? OrderItems { get; set; }
        public DbSet<Invoice>? Invoices { get; set; }
        public DbSet<InvoiceItem>? InvoiceItems { get; set; }
    }
    [Serializable]
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
    [Serializable]
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
    [Serializable]
    public enum PropertyManagerRoles { Administrator }
    [Serializable]
    public class Offering
    {
        public Offering()
        {
            Name = "";
            Slug = "";
            Description = "";
            InventoryItem = new();
        }
        public int OfferingId { get; set; }
        public InventoryItem InventoryItem { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName="decimal(18,2)")]
        public decimal Price { get; set; }
    }
    [Serializable]
    public class OfferingManager
    {
        public OfferingManager()
        {
            Offering = new();
        }
        public int OfferingManagerId { get; set; }
        public Offering Offering { get; set; }
        public string? UserId { get; set; }
        public OfferingManagerRoles Role { get; set; }
    }
    [Serializable]
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
    [Serializable]
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
    [Serializable]
    public class PropertyOfferingMapping
    {
        public int PropertyOfferingMappingId { get; set; }
        public Property? Property { get; set; }
        public Offering? Offering { get; set; }
    }
    [Serializable]
    public enum OfferingManagerRoles { Administrator }
    [Serializable]
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
    [Serializable]
    public class Inventory
    {
        public Inventory()
        {
            Name = "";
        }
        public int InventoryId { get; set; }
        public string Name { get; set; }
    }
    [Serializable]
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
    }
    [Serializable]
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
    [Serializable]
    public enum InventoryManagerRoles { Administrator };

    public class Order
    {
        public int OrderId { get; set; }
        public string? UserId { get; set; }
        public OrderState State { get; set; }
    }
    public enum OrderState { Incomplete, Complete }
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public Order? Order { get; set; }
        public Property? Property { get; set; }
        public Offering? Offering { get; set; }
        public int Quantity { get; set; }
    }
    public class OfferingWithOrderItem
    {
        public Offering? Offering { get; set; }
        public OrderItem? OrderItem { get; set; }
    }
    public class OfferingWithProperty
    {
        public Offering? Offering;
        public Property? Property;
    }
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string UserId { get; set; }
        public Property? Property { get; set; }
        public Order? Order { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }
        public Invoice? Invoice { get; set; }
        public string? Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}