namespace EntityEquity.Data.Models
{
    public class OfferingWithOrderItem
    {
        public Offering? Offering { get; set; }
        public OrderItem? OrderItem { get; set; }
        public List<PhotoUrl>? Photos { get; set; }
    }
    public class OfferingWithProperty
    {
        public Offering? Offering;
        public Property? Property;
        public List<PhotoUrl>? Photos;
    }
    public class OfferingWithInventoryItem
    {
        public Offering? Offering;
        public InventoryItem? InventoryItem;
    }
    public class Shareholder
    {
        public int PropertyId;
        public string PropertyName;
        public int PropertyShares;
        public string UserId;
        public int Shares;
    }
    public class PlatformFee
    {
        public int PlatformId;
        public string UserId;
        public decimal Amount;
    }
}
