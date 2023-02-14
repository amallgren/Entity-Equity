using EntityEquity.Data;
using EntityEquity.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace EntityEquity.Pages
{
    public class PropIndexModel : PageModel
    {
        public string Slug { get; set; }
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public PropIndexModel(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public void OnGet(string slug)
        {
            Slug = slug;
        }

        public Property Property
        {
            get
            {
                return GetProperty();
            }
        }
        private Property GetProperty()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var property = (from p in dbContext.Properties
                                where p.Slug == Slug
                                select p).FirstOrDefault();
                return property;
            }
            
        }
    }
}
