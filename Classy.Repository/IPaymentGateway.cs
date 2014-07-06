using Classy.Models;

namespace Classy.Repository
{
    public interface IPaymentGateway
    {
        Transaction Charge(string appId, decimal amount, string currency, PaymentMethod paymentMethod, bool doCapture);
        Transaction Refund(string appId, string transactionId, decimal amount);
    }
}
