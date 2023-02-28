using System.ComponentModel.DataAnnotations;

namespace EntityEquity.Models
{
    public class ShippingAddressModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string ZipCode { get; set; }
    }
}
