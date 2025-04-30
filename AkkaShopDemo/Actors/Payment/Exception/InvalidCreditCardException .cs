namespace AkkaShopDemo.Actors.Payment.Exception
{
    public class InvalidCreditCardException : CreditCardException
    {
        public InvalidCreditCardException() : base("Credit card info is invalid.") { }
    }
}
