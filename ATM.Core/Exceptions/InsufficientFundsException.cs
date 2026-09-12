using System;
using System.Runtime.Serialization;

namespace ATM.Core.Exceptions
{
    [Serializable]
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException()
        {
        }

        public InsufficientFundsException(string message)
            : base(message)
        {
        }

        public InsufficientFundsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InsufficientFundsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static InsufficientFundsException Create(string message)
        {
            // Lightweight logging for visibility; keep behavior minimal
            Console.WriteLine("Insufficient funds: " + message);
            return new InsufficientFundsException(message);
        }
    }
}
