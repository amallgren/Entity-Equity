using EntityEquity.Data;
using System.ComponentModel.DataAnnotations;

namespace EntityEquity.Models
{
    public class PropertyModel
    {
        public PropertyModel()
        {
            Name = "";
            Slug = "";
            PropertyManagers = new();
        }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Slug { get; set; }
        public List<PropertyManager> PropertyManagers { get; set; }
    }
}
