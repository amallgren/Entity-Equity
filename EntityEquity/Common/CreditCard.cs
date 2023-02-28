using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.EntityFrameworkCore;
using EntityEquity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EntityEquity.Common
{
    public class CreditCardCharge
    {
        private IConfiguration _configuration;
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private UserManager<IdentityUser> _userManager;
        private ClaimsPrincipal _user;
        public CreditCardCharge(IConfiguration configuration, 
            IDbContextFactory<ApplicationDbContext> dbContextFactory, 
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user)
        {
            _configuration = configuration;
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _user = user;
        }
        public async Task<CreditCardResult> Run(CreditCardRunParameters parameters)
        {
            try
            {
                IConfigurationSection authorizeNetConfig = _configuration.GetSection("AuthorizeNet");
                var apiLoginId = authorizeNetConfig.GetValue<string>("ApiLoginId");
                var apiTransactionKey = authorizeNetConfig.GetValue<string>("ApiTransactionKey");
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

                // define the merchant information (authentication / transaction id)
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
                {
                    name = apiLoginId,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = apiTransactionKey
                };

                var creditCard = parameters.CreditCard;
                var billingAddress = parameters.BillingAddress;
                var paymentType = new paymentType { Item = creditCard };
                var lineItems = parameters.LineItems.ToArray();
                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                    amount = parameters.Amount,
                    payment = paymentType,
                    billTo = billingAddress,
                    lineItems = lineItems
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };
                var controller = new createTransactionController(request);
                controller.Execute();

                var response = controller.GetApiResponse();

                Data.CommonDataSets.Orders orderDataSet = new(_dbContextFactory, _userManager, _user);
                var order = orderDataSet.GetIncompleteOrder();

                var result = await ProcessResponse(response, order);

                if (response is null)
                {
                    
                    var errorResponse = controller.GetErrorResponse();
                    PaymentTransactionError error = new PaymentTransactionError()
                    {
                        Order = order,
                        ErrorCode = errorResponse.messages.resultCode.ToString(),
                        ErrorMessage = errorResponse.messages.message.ToString()
                    };
                    LogPaymentTransactionError(error);
                    result.Error = error;
                }
                return result;
            }
            catch (Exception ex)
            {
                string breakpoint = "";
            }
            return null;
        }
        private async Task LogPaymentTransactionError(PaymentTransactionError error)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                dbContext.Orders.Attach(error.Order);
                dbContext.PaymentTransactionErrors.Add(error);
                await dbContext.SaveChangesAsync();
            }
        }
        private async Task<CreditCardResult> ProcessResponse(createTransactionResponse response, Order order)
        {
            CreditCardResult result = new();
            result.Successful = false;
            PaymentTransactionError error = new PaymentTransactionError()
            {
                Order = order,
                ErrorCode = "Unknown",
                ErrorMessage = "Transaction failed."
            };
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                dbContext.Orders.Attach(error.Order);
                if (response is not null)
                {
                    if (response.messages.resultCode == messageTypeEnum.Ok)
                    {
                        if (response.transactionResponse.messages is not null)
                        {
                            PaymentTransaction paymentTransaction = new PaymentTransaction()
                            {
                                Order = order,
                                TransactionId = response.transactionResponse.transId,
                                ResponseCode = response.transactionResponse.responseCode,
                                MessageCode = response.transactionResponse.messages[0].code,
                                Description = response.transactionResponse.messages[0].description,
                                AuthorizationCode = response.transactionResponse.authCode
                            };
                            dbContext.Orders.Attach(paymentTransaction.Order);
                            dbContext.PaymentTransactions!.Add(paymentTransaction);
                            result.Successful = true;
                        }
                        else
                        {
                            if (response.transactionResponse.errors != null)
                            {
                                error.ErrorCode = response.transactionResponse.errors[0].errorCode;
                                error.ErrorMessage = response.transactionResponse.errors[0].errorText;
                            }
                            dbContext.PaymentTransactionErrors.Add(error);
                        }
                    }
                    else
                    {
                        if (response.transactionResponse != null && response.transactionResponse.errors != null)
                        {
                            error.ErrorCode = response.transactionResponse.errors[0].errorCode;
                            error.ErrorMessage = response.transactionResponse.errors[0].errorText;
                        }
                        else
                        {
                            error.ErrorCode = response.messages.message[0].code;
                            error.ErrorMessage = response.messages.message[0].text;
                        }
                        dbContext.PaymentTransactionErrors.Add(error);
                    }
                }
                else
                {
                    error.ErrorCode = "Null";
                    error.ErrorMessage = "Response was null.";

                    dbContext.PaymentTransactionErrors.Add(error);
                }
                await dbContext.SaveChangesAsync();
            }
            if (!result.Successful)
                result.Error = error;
            return result;
        }

    }
    public class CreditCardRunParameters
    {
        public creditCardType CreditCard { get; set; }
        public customerAddressType BillingAddress { get; set; }
        public List<lineItemType> LineItems { get; set; }
        public decimal Amount { get; set; }
    }
    public class CreditCardResult
    {
        public bool Successful { get; set; }
        public PaymentTransactionError? Error { get; set; }
    }
}
