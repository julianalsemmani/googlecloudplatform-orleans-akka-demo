using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.Payment.Exception;
using OrleansShopDemo.Grains.Payment.Interfaces;
using OrleansShopDemo.Grains.Shipping;
using Shop;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrleansShopDemo.Grains.Payment
{
    public class PaymentGrain : Grain, IPaymentGrain
    {
        private readonly ILogger<PaymentGrain> logger;
        public PaymentGrain(ILogger<PaymentGrain> logger)
        {
            this.logger = logger;
        }

        public Task<ChargeResponse> Charge(ChargeRequest request)
        {
            logger.LogInformation("Charge requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Processing charge for {Amount}", request.Amount);

            var creditCard = request.CreditCard;
            var cardNumber = creditCard.CreditCardNumber;
            var cardType = GetCardType(cardNumber);

            if (string.IsNullOrEmpty(cardType))
            {
                throw new InvalidCreditCardException();
            }

            if (!(cardType == "visa" || cardType == "mastercard"))
            {
                throw new UnacceptedCreditCardException(cardType);
            }

            var currentDateTime = DateTime.Now;
            var currentDate = new DateOnly(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day);
            var cardExpirationDate = new DateOnly(creditCard.CreditCardExpirationYear, creditCard.CreditCardExpirationMonth, 1);

            if (cardExpirationDate.AddMonths(1).AddDays(-1) < currentDate)
            {
                throw new ExpiredCreditCardException(cardNumber, creditCard.CreditCardExpirationMonth, creditCard.CreditCardExpirationYear);
            }

            var transactionId = Guid.NewGuid();

            Console.WriteLine($"Transaction processed: {cardType} ending {cardNumber[^4..]} " +
                              $"Amount: {request.Amount.CurrencyCode}{request.Amount.Units}.{request.Amount.Nanos}");

            return Task.FromResult(new ChargeResponse { TransactionId = transactionId.ToString() });
        }

        private static string? GetCardType(string cardNumber)
        {
            if (Regex.IsMatch(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$"))
                return "visa";
            if (Regex.IsMatch(cardNumber, @"^5[1-5][0-9]{14}$"))
                return "mastercard";

            return null;
        }
    }
}
