using System.ComponentModel.DataAnnotations;
using EntityEquity.Data;
using EntityEquity.Extensions;

namespace EntityEquity.Models
{
    public class OfferingModel
    {
        public OfferingModel()
        {
            Slug = "";
            Name = "";
            Description = "";
            PropertyIdsStrings = new string[0];
            InventoryItemIdStrings = new string[0];
        }
        [Required]
        public string Slug { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string[] PropertyIdsStrings { get; set; }
        public int[] PropertyIds
        {
            get
            {
                return PropertyIdsStrings.ToIntArray();
            }
        }
        [Required]
        public string[] InventoryItemIdStrings { get; set; }
        public int[] InventoryItemIds { 
            get
            {
                return InventoryItemIdStrings.ToIntArray();
            }
        }
        public decimal Price { get; set; }
        public List<OfferingManager> OfferingManagers { get; set; }
    }
}
