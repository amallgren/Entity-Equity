using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;

namespace EntityEquity.Models
{
    public class IndexModel
    {
        private ApplicationDbContext? _context;
        public string BaseAddress { get; set; }
        public List<Property>? Properties { get; set; }
        public List<EquityShare>? EquityShares { get; set; }
        public string UserID { get; set; }
        public IndexModel(string baseAddress, string userId)
        {
            BaseAddress = baseAddress;
            UserID = userId;
        }
        public void Fill(ApplicationDbContext context)
        {
            _context = context;
            FillProperties();
            FillEquityShares();
        }
        private void FillProperties()
        {
            if (_context != null)
            {
                Properties = (from p in _context.Properties
                             join pm in _context.PropertyManagers!
                                on p.PropertyId equals pm.Property.PropertyId
                             where pm.Role == PropertyManagerRoles.Administrator
                                && pm.UserId == UserID
                             select p).ToList();
            }
        }
        private void FillEquityShares()
        {

        }
    }
}
