namespace AkkaShopDemo.Actors.Payment.Exception
{
    public class CreditCardException : System.Exception
    {
        public int Code { get; }

        public CreditCardException(string message, int code = 400) : base(message)
        {
            Code = code;
        }
    }
}
