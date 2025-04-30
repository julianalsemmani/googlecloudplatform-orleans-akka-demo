namespace OrleansShopDemo.Grains.Payment.Exception
{
    public class ExpiredCreditCardException : CreditCardException
    {
        public ExpiredCreditCardException(string number, int month, int year)
            : base($"Your credit card (ending {number[^4..]}) expired on {month}/{year}.") { }
    }
}
