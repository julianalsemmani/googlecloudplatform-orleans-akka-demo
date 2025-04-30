using Microsoft.Extensions.Logging;
using AkkaShopDemo.Actors.Payment.Exception;
using Shop;
using System;
using System.Text.RegularExpressions;
using Akka.Actor;

namespace AkkaShopDemo.Actors.Payment
{
    public class PaymentActor : ReceiveActor
    {
        private readonly Guid primaryKey = Guid.NewGuid();
        private readonly ILogger<PaymentActor> logger;
        public PaymentActor(ILogger<PaymentActor> logger)
        {
            this.logger = logger;
            Receive<ChargeRequest>(Charge);
        }

        public void Charge(ChargeRequest request)
        {
            logger.LogInformation("Charge requested with {Id}", primaryKey.ToString());
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

            Sender.Tell(new ChargeResponse { TransactionId = transactionId.ToString() });
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
