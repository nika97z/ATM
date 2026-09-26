namespace ATM.Core.Exceptions
{
    public class LoanExeption : Exception
    {
        public LoanExeption()
        {
        }

        public LoanExeption(string? message) : base(message)
        {
        }
    }
}
