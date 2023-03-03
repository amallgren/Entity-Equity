using AuthorizeNet.Api.Contracts.V1;
using EntityEquity.Common;
namespace EntityEquity.Models.Mapping
{
    public static class PaymentForm
    {
        public static CreditCardPaymentParameters MapCreditCard(CreditCardPaymentFormModel model, List<lineItemType> lineItems, decimal total)
        {
            var creditCard = new creditCardType
            {
                cardNumber = model.CreditCardNumber,
                expirationDate = model.CreditCardExpirationDate,
                cardCode = model.CreditCardSecurityCode
            };
            var billingAddress = new customerAddressType
            {
                firstName = model.BillingFirstName,
                lastName = model.BillingLastName,
                address = model.BillingAddress,
                city = model.BillingCity,
                zip = model.BillingZipCode
            };
            CreditCardPaymentParameters parameters = new()
            {
                CreditCard = creditCard,
                BillingAddress = billingAddress,
                LineItems = lineItems,
                Amount = total
            };
            return parameters;
        }
        public static eCheckPaymentParameters MapECheck(eCheckPaymentFormModel model, decimal total)
        {
            var bankAccount = new bankAccountType
            {
                routingNumber = model.RoutingNumber.ToString(),
                accountNumber = model.AccountNumber.ToString(),
                nameOnAccount = model.Name,
                bankName = model.BankName,
                accountType = model.AccountType,
                echeckType = echeckTypeEnum.WEB
            };

            return new eCheckPaymentParameters
            {
                BankAccount = bankAccount,
                Amount = total
            };
        }
    }
}
