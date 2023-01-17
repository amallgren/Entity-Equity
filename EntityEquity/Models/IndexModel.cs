using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Models
{
    public class IndexModel
    {
        private ApplicationDbContext? _context;
        public List<Property>? Properties { get; set; }
        public List<EquityShare>? EquityShares { get; set; }
        public void Fill(ApplicationDbContext context, string userId)
        {
            _context = context;
            FillProperties(userId);
            FillEquityShares(userId);
        }
        private void FillProperties(string userId)
        {
            if (_context != null)
            {
                Properties = (from p in _context.Properties
                             join pm in _context.PropertyManagers!
                                on p.PropertyId equals pm.Property.PropertyId
                             where pm.Role == PropertyManagerRoles.Administrator
                                && pm.UserId == userId
                             select p).ToList();
            }
        }
        private void FillEquityShares(string userId)
        {

        }
    }
}
