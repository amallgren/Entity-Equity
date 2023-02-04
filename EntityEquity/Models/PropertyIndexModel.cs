using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Models
{
    public class PropertyIndexModel
    {
        public string BaseAddress { get; set; }
        public List<Property>? Properties { get; set; }
        public List<EquityShare>? EquityShares { get; set; }
        public string UserId { get; set; }
        public PropertyIndexModel(string baseAddress, string userId)
        {
            BaseAddress = baseAddress;
            UserId = userId;
        }
    }
}