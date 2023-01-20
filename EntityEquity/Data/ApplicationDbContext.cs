using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        }
        public int OfferingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class PropertyOfferingMapping
    {
        public int PropertyOfferingMappingId { get; set; }
        public Property? Property { get; set; }
        public Offering? Offering { get; set; }
    }
    public class EquityShare
    {
        public int EquityShareId { get; set; }
        public Property? Property { get; set; }
        public string? UserId { get; set; }
        public decimal Percentage { get; set; }
    }
}