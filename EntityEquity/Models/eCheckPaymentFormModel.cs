using AuthorizeNet.Api.Contracts.V1;
using System.ComponentModel.DataAnnotations;

namespace EntityEquity.Models
{
    public class eCheckPaymentFormModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int RoutingNumber { get; set; }
        [Required]
        public int AccountNumber { get; set; }
        [Required]
        public bankAccountTypeEnum AccountType { get; set; }
        [Required]
        public string BankName { get; set; }
    }
}
