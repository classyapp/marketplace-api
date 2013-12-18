using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Repository
{
    public class TranzilaPaymentGateway : IPaymentGateway
    {
        private ITransactionRepository TransactionRepository;

        public TranzilaPaymentGateway(
            ITransactionRepository transactionRepository)
        {
            TransactionRepository = transactionRepository;
        }
 
        private bool ValidateGatewayResponse(PaymentGatewayResponse response, bool throwIfInvalid = true)
        {
            if (response.StatusCode != "A-OK")
            {
                if (throwIfInvalid) throw new PaymentGatewayException(response);
                return false;
            }
            else return true;
        }

        public Transaction Charge(string appId, double amount, string currency, PaymentMethod paymentMethod, bool doCapture)
        {
            // authorize the amount in tranzilla
            var gatewayResponse =  new PaymentGatewayResponse
            {
                GatewayRefId = "123123",
                StatusCode = "A-OK",
                StatusReason = "cause i like it!"
            };
            ValidateGatewayResponse(gatewayResponse);

            // create transaction object
            var transaction = new Transaction
            {
                AppId = appId,
                Amount = amount,
                Currency = currency,
                GatewayRefId = gatewayResponse.GatewayRefId
            };

            // capture the amount in tranzilla
            gatewayResponse = new PaymentGatewayResponse
            {
                GatewayRefId = "123123",
                StatusCode = "A-OK",
                StatusReason = "cause i like it!"
            };
            var captureSuccessful = ValidateGatewayResponse(gatewayResponse);
            if (captureSuccessful) {
                transaction.Capture = new SubTransaction
                {
                    Amount = amount,
                    GatewayRefId = gatewayResponse.GatewayRefId
                };
            }

            // save the transaction
            TransactionRepository.Save(transaction);

            return transaction;
        }

        public Transaction Refund(string appId, string transactionId, double amount)
        {
            var transaction = TransactionRepository.GetById(appId, transactionId);
            if (transaction == null) throw new KeyNotFoundException("invalid transaction");

            // refund in tranzilla
            var gatewayResponse = new PaymentGatewayResponse
            {
                GatewayRefId = "123123",
                StatusCode = "A-OK",
                StatusReason = "cause i like it!"
            };

            // save the refund transaction as a subtransaction
            if (transaction.Refunds == null) transaction.Refunds = new List<SubTransaction>();
            transaction.Refunds.Add(new SubTransaction
            {
                Amount = amount,
                GatewayRefId = gatewayResponse.GatewayRefId
            });
            TransactionRepository.Save(transaction);

            return transaction;
        }
    }
}