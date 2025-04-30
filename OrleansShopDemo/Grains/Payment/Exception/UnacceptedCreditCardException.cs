namespace OrleansShopDemo.Grains.Payment.Exception
{
    public class UnacceptedCreditCardException : CreditCardException
    {
        public UnacceptedCreditCardException(string cardType)
            : base($"Sorry, we cannot process {cardType} credit cards. Only VISA or MasterCard is accepted.") { }
    }
}
