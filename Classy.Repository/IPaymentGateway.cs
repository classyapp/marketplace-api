using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    public interface IPaymentGateway
    {
        Transaction Charge(string appId, double amount, string currency, PaymentMethod paymentMethod, bool doCapture);
        Transaction Refund(string appId, string transactionId, double amount);
    }
}
